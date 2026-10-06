namespace bendita_ajuda_backend.Services;

/// <summary>
/// Normalização de números de celular. Todo o sistema salva e compara
/// o número no formato normalizado: somente dígitos, DDD + número, sem o 55.
/// </summary>
public static class Celular
{
    /// <summary>
    /// Ex.: "+55 (61) 99999-8888" → "61999998888".
    /// Retorna null se o número não for válido.
    /// </summary>
    public static string? Normalizar(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            return null;

        var digitos = new string(numero.Where(char.IsAsciiDigit).ToArray());

        if (digitos.StartsWith("55") && digitos.Length is 12 or 13)
            digitos = digitos[2..];

        var ehValido = digitos.Length is 10 or 11 && digitos[0] != '0';
        return ehValido ? digitos : null;
    }
}
