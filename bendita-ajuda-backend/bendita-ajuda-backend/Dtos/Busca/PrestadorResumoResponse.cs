namespace bendita_ajuda_backend.Dtos.Busca;

/// <summary>Prestador como o cliente vê (card da busca e página do prestador). O CEP nunca vem.</summary>
public record PrestadorResumoResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? FotoUrl { get; init; }
    public string? Bio { get; init; }
    public string? Bairro { get; init; }
    public string Cidade { get; init; } = string.Empty;
    public string Uf { get; init; } = string.Empty;

    /// <summary>Os que combinaram com a busca vêm primeiro.</summary>
    public IReadOnlyList<ServicoResumoResponse> Servicos { get; init; } = [];

    /// <summary>Só para quem entrou: protege o número do prestador de robôs e curiosos.</summary>
    public string? Whatsapp { get; init; }
}

public record ServicoResumoResponse
{
    public string Id { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
}
