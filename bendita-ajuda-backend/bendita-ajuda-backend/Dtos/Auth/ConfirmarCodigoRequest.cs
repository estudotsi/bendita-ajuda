using System.ComponentModel.DataAnnotations;

namespace bendita_ajuda_backend.Dtos.Auth;

public record ConfirmarCodigoRequest
{
    [Required(ErrorMessage = "Informe o número do celular.")]
    [StringLength(30, ErrorMessage = "Esse número de celular está grande demais.")]
    public string Celular { get; init; } = string.Empty;

    [Required(ErrorMessage = "Informe o código que você recebeu.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "O código tem 6 números.")]
    public string Codigo { get; init; } = string.Empty;

    /// <summary>Só é necessário no primeiro acesso (quando a resposta foi precisaNome: true).</summary>
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Escreva seu nome com 2 a 100 letras.")]
    public string? Nome { get; init; }
}
