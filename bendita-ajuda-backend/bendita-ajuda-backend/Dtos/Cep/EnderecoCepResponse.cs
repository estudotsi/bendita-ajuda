using bendita_ajuda_backend.Services.Cep;

namespace bendita_ajuda_backend.Dtos.Cep;

public record EnderecoCepResponse(string Cep, string? Bairro, string Cidade, string Uf)
{
    public static EnderecoCepResponse De(EnderecoCep e) => new(e.Cep, e.Bairro, e.Cidade, e.Uf);
}
