namespace bendita_ajuda_backend.Dtos.Servicos;

public record ServicoResponse
{
    public string Id { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string NomePlural { get; init; } = string.Empty;
}
