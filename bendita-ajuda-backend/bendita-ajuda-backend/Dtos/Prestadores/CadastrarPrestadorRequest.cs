using System.ComponentModel.DataAnnotations;

namespace bendita_ajuda_backend.Dtos.Prestadores;

public record CadastrarPrestadorRequest
{
    /// <summary>Ids dos serviços escolhidos nos botões (ex.: ["eletricista"]).</summary>
    [MaxLength(10, ErrorMessage = "Escolha no máximo 10 serviços.")]
    public List<string> Servicos { get; init; } = [];

    /// <summary>O que ele digitou em "Não achei meu serviço". Opcional.</summary>
    [StringLength(100, ErrorMessage = "Escreva o seu serviço com até 100 letras.")]
    public string? OutroServico { get; init; }

    [Required(ErrorMessage = "Informe o CEP.")]
    [StringLength(9, ErrorMessage = "O CEP tem 8 números.")]
    public string Cep { get; init; } = string.Empty;

    /// <summary>Pode corrigir o bairro que veio do CEP, ou deixar vazio.</summary>
    [StringLength(100, ErrorMessage = "Escreva o bairro com até 100 letras.")]
    public string? Bairro { get; init; }
}
