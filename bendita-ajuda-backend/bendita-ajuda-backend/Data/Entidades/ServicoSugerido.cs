namespace bendita_ajuda_backend.Data.Entidades;

/// <summary>
/// Serviço que o prestador digitou por não achar na lista. Existe só enquanto está pendente:
/// quando o admin resolve (mesmo serviço, serviço novo ou recusa), a linha é apagada.
/// </summary>
public class ServicoSugerido
{
    public Guid Id { get; set; }

    /// <summary>Como o prestador escreveu (ex.: "formatador de computador").</summary>
    public string Descricao { get; set; } = string.Empty;

    public Guid PrestadorId { get; set; }

    public Prestador Prestador { get; set; } = null!;

    public DateTime CriadoEm { get; set; }
}
