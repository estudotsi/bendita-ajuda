using System.ComponentModel.DataAnnotations;

namespace bendita_ajuda_backend.Dtos.Auth;

public record EnviarCodigoRequest
{
    [Required(ErrorMessage = "Informe o número do celular.")]
    [StringLength(30, ErrorMessage = "Esse número de celular está grande demais.")]
    public string Celular { get; init; } = string.Empty;
}
