using bendita_ajuda_backend.Dtos.Servicos;

namespace bendita_ajuda_backend.Dtos.Busca;

public record BuscaPrestadoresResponse
{
    /// <summary>
    /// Serviços que a busca entendeu (o do botão, ou os que combinaram com o texto).
    /// Vazio com texto preenchido = não entendemos o que a pessoa precisa.
    /// </summary>
    public IReadOnlyList<ServicoResponse> ServicosEncontrados { get; init; } = [];

    public IReadOnlyList<PrestadorResumoResponse> Prestadores { get; init; } = [];
}
