namespace bendita_ajuda_backend.Services.EnvioCodigo;

/// <summary>
/// Envia o código por SMS pela API da TrackMax (POST /api/v1/sms).
/// O HttpClient já vem com a URL base e o token configurados no Program.cs.
/// </summary>
public class EnvioCodigoSms(HttpClient http, ILogger<EnvioCodigoSms> logger) : IEnvioCodigo
{
    public async Task<bool> EnviarAsync(string celular, string codigo, CancellationToken cancellationToken)
    {
        // Sem acentos: SMS com acento cai para 70 caracteres e alguns aparelhos mostram errado.
        var mensagem = $"Bendita Ajuda: seu codigo e {codigo}. Vale por 10 minutos. Nao passe para ninguem.";
        var requisicao = new RequisicaoSms($"55{celular}", mensagem, Interaction: false);

        try
        {
            using var resposta = await http.PostAsJsonAsync("api/v1/sms", requisicao, cancellationToken);
            var corpo = await resposta.Content.ReadAsStringAsync(cancellationToken);

            if (!resposta.IsSuccessStatusCode)
            {
                logger.LogError("Falha ao enviar SMS para {Celular}: HTTP {Status} {Corpo}",
                    celular, (int)resposta.StatusCode, corpo);
                return false;
            }

            logger.LogInformation("SMS de verificação enviado para {Celular}: {Corpo}", celular, corpo);
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Erro de rede ao enviar SMS para {Celular}.", celular);
            return false;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Tempo esgotado ao enviar SMS para {Celular}.", celular);
            return false;
        }
    }

    private record RequisicaoSms(string Number, string Message, bool Interaction);
}
