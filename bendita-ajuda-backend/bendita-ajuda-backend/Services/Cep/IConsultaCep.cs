namespace bendita_ajuda_backend.Services.Cep;

public record EnderecoCep(string Cep, string? Bairro, string Cidade, string Uf);

public enum StatusConsultaCep
{
    Encontrado,
    NaoEncontrado,
    Indisponivel,
}

public record ResultadoConsultaCep(StatusConsultaCep Status, EnderecoCep? Endereco = null);

/// <summary>Descobre bairro, cidade e UF a partir do CEP.</summary>
public interface IConsultaCep
{
    /// <param name="cep">Somente os 8 dígitos (use <see cref="CepUtil.Normalizar"/>).</param>
    Task<ResultadoConsultaCep> ConsultarAsync(string cep, CancellationToken cancellationToken);
}

public static class CepUtil
{
    /// <summary>Ex.: "70.040-010" → "70040010". Retorna null se não tiver 8 dígitos.</summary>
    public static string? Normalizar(string? cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            return null;

        var digitos = new string(cep.Where(char.IsAsciiDigit).ToArray());
        return digitos.Length == 8 && digitos != "00000000" ? digitos : null;
    }
}
