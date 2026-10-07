using bendita_ajuda_backend.Dtos.Servicos;

namespace bendita_ajuda_backend.Dtos.Prestadores;

/// <summary>O cadastro de prestador de quem está logado.</summary>
public record MeuCadastroResponse
{
    public string Cep { get; init; } = string.Empty;
    public string? Bairro { get; init; }
    public string Cidade { get; init; } = string.Empty;
    public string Uf { get; init; } = string.Empty;
    public bool Visivel { get; init; }
    public IReadOnlyList<ServicoResponse> Servicos { get; init; } = [];

    /// <summary>Serviços digitados que o admin ainda não analisou ("em análise").</summary>
    public IReadOnlyList<string> SugestoesPendentes { get; init; } = [];
}
