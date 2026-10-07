namespace bendita_ajuda_backend.Dtos.Admin;

/// <summary>Uma linha da tela de sugestões do admin.</summary>
public record SugestaoPendenteResponse
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; }
    public string PrestadorNome { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Uf { get; init; } = string.Empty;
}
