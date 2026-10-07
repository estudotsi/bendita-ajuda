using System.ComponentModel.DataAnnotations;

namespace bendita_ajuda_backend.Dtos.Admin;

/// <summary>Tela do admin: o que as pessoas procuraram e não acharam.</summary>
public record BuscasSemResultadoResponse
{
    /// <summary>Textos que não combinaram com nenhum serviço. As mais repetidas primeiro.</summary>
    public IReadOnlyList<BuscaNaoEntendidaResponse> NaoEntendidas { get; init; } = [];

    /// <summary>Serviços procurados em cidades onde ainda não há prestador deles.</summary>
    public IReadOnlyList<BuscaSemPrestadorResponse> SemPrestador { get; init; } = [];
}

public record BuscaNaoEntendidaResponse
{
    public int Id { get; init; }
    public string Texto { get; init; } = string.Empty;
    public int Quantidade { get; init; }
    public DateTime UltimaVez { get; init; }
}

public record BuscaSemPrestadorResponse
{
    public string ServicoId { get; init; } = string.Empty;
    public string ServicoNome { get; init; } = string.Empty;
    /// <summary>Vazio quando quem buscou não tinha informado o CEP.</summary>
    public string Cidade { get; init; } = string.Empty;
    public string Uf { get; init; } = string.Empty;
    public int Quantidade { get; init; }
    public DateTime UltimaVez { get; init; }
}

/// <summary>"É o mesmo que...": ensina uma palavra da busca a um serviço.</summary>
public record EnsinarPalavraRequest
{
    [Required(ErrorMessage = "Escolha o serviço.")]
    [StringLength(50)]
    public string ServicoId { get; init; } = string.Empty;

    /// <summary>A palavra que vai virar palavra-chave (o admin pode encurtar o texto buscado).</summary>
    [Required(ErrorMessage = "Escreva a palavra.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "A palavra deve ter de 2 a 100 letras.")]
    public string Palavra { get; init; } = string.Empty;
}
