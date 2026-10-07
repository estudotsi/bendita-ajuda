using System.Globalization;
using System.Text;

namespace bendita_ajuda_backend.Services;

/// <summary>Normalização de textos digitados (serviços, palavras-chave).</summary>
public static class Texto
{
    /// <summary>
    /// Minúsculas, sem acentos, só letras/números e espaços simples.
    /// Ex.: "  Técnico de Ar-Condicionado " → "tecnico de ar condicionado".
    /// </summary>
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        var sb = new StringBuilder(texto.Length);
        var espacoPendente = false;
        foreach (var c in texto.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c))
            {
                if (espacoPendente && sb.Length > 0)
                    sb.Append(' ');
                sb.Append(char.ToLowerInvariant(c));
                espacoPendente = false;
            }
            else
            {
                espacoPendente = true;
            }
        }
        return sb.ToString();
    }

    /// <summary>Identificador para URL. Ex.: "Técnico de informática" → "tecnico-de-informatica".</summary>
    public static string Slug(string texto, int tamanhoMaximo)
    {
        var slug = Normalizar(texto).Replace(' ', '-');
        return slug.Length <= tamanhoMaximo ? slug : slug[..tamanhoMaximo].TrimEnd('-');
    }

    /// <summary>Primeira letra maiúscula, o resto como veio. Ex.: "boleira" → "Boleira".</summary>
    public static string PrimeiraMaiuscula(string texto)
    {
        texto = texto.Trim();
        return texto.Length == 0 ? texto : char.ToUpper(texto[0], new CultureInfo("pt-BR")) + texto[1..];
    }
}
