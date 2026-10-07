using System.Text.Json;

namespace bendita_ajuda_backend.Services.Cep;

/// <summary>
/// Consulta o ViaCEP (gratuito, sem chave): GET https://viacep.com.br/ws/{cep}/json/.
/// O HttpClient já vem com a URL base configurada no Program.cs.
/// </summary>
public class ConsultaCepViaCep(HttpClient http, ILogger<ConsultaCepViaCep> logger) : IConsultaCep
{
    public async Task<ResultadoConsultaCep> ConsultarAsync(string cep, CancellationToken cancellationToken)
    {
        try
        {
            using var resposta = await http.GetAsync($"ws/{cep}/json/", cancellationToken);

            // CEP com formato aceito mas inexistente: o ViaCEP responde 200 com { "erro": true }.
            if (resposta.StatusCode == System.Net.HttpStatusCode.BadRequest)
                return new ResultadoConsultaCep(StatusConsultaCep.NaoEncontrado);

            if (!resposta.IsSuccessStatusCode)
            {
                logger.LogError("ViaCEP respondeu HTTP {Status} para o CEP {Cep}.", (int)resposta.StatusCode, cep);
                return new ResultadoConsultaCep(StatusConsultaCep.Indisponivel);
            }

            using var json = await JsonDocument.ParseAsync(
                await resposta.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var raiz = json.RootElement;

            if (raiz.TryGetProperty("erro", out _))
                return new ResultadoConsultaCep(StatusConsultaCep.NaoEncontrado);

            var cidade = Ler(raiz, "localidade");
            var uf = Ler(raiz, "uf");
            if (cidade is null || uf is null)
                return new ResultadoConsultaCep(StatusConsultaCep.NaoEncontrado);

            // CEP único da cidade (cidades pequenas) vem sem bairro.
            var endereco = new EnderecoCep(cep, Ler(raiz, "bairro"), cidade, uf.ToUpperInvariant());
            return new ResultadoConsultaCep(StatusConsultaCep.Encontrado, endereco);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException
            || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            logger.LogError(ex, "Falha ao consultar o CEP {Cep} no ViaCEP.", cep);
            return new ResultadoConsultaCep(StatusConsultaCep.Indisponivel);
        }
    }

    private static string? Ler(JsonElement raiz, string campo) =>
        raiz.TryGetProperty(campo, out var valor) && valor.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(valor.GetString())
            ? valor.GetString()!.Trim()
            : null;
}
