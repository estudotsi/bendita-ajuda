using System.ComponentModel.DataAnnotations;

namespace bendita_ajuda_backend.Dtos.Admin;

/// <summary>"É o mesmo que...": liga o prestador a um serviço que já existe.</summary>
public record SugestaoSimilarRequest
{
    [Required(ErrorMessage = "Escolha o serviço.")]
    [StringLength(50)]
    public string ServicoId { get; init; } = string.Empty;
}

/// <summary>Cria um serviço novo a partir da sugestão.</summary>
public record SugestaoNovoServicoRequest
{
    [Required(ErrorMessage = "Escreva o nome do serviço.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome do serviço deve ter de 2 a 100 letras.")]
    public string Nome { get; init; } = string.Empty;

    [Required(ErrorMessage = "Escreva o nome no plural.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O plural deve ter de 2 a 100 letras.")]
    public string NomePlural { get; init; } = string.Empty;
}
