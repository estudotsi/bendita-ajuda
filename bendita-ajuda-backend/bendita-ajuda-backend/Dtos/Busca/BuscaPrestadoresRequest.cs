using System.ComponentModel.DataAnnotations;

namespace bendita_ajuda_backend.Dtos.Busca;

/// <summary>Filtros da busca (query string). Tudo opcional: sem nada, lista quem está perto.</summary>
public record BuscaPrestadoresRequest
{
    /// <summary>Id do serviço tocado nos botões. Quando vem, o texto é ignorado.</summary>
    [StringLength(50)]
    public string? Servico { get; init; }

    /// <summary>O que a pessoa escreveu ou falou.</summary>
    [StringLength(100, ErrorMessage = "Escreva com menos palavras.")]
    public string? Texto { get; init; }

    /// <summary>Onde a pessoa está (vem do CEP dela). Sem UF, a busca é no Brasil todo.</summary>
    [StringLength(100)]
    public string? Bairro { get; init; }

    [StringLength(100)]
    public string? Cidade { get; init; }

    [StringLength(2)]
    public string? Uf { get; init; }
}
