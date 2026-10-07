using bendita_ajuda_backend.Dtos;
using bendita_ajuda_backend.Dtos.Cep;
using bendita_ajuda_backend.Services.Cep;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace bendita_ajuda_backend.Controllers;

[ApiController]
[Route("api/cep")]
// Aberto: o cliente informa o CEP para ver quem está perto, sem precisar entrar.
// O limite por IP evita usar a API como consulta de CEP grátis.
[EnableRateLimiting(PoliticaCep)]
public class CepController(IConsultaCep consultaCep) : ControllerBase
{
    /// <summary>Nome da política do rate limiter (por IP) da consulta de CEP.</summary>
    public const string PoliticaCep = "cep";

    public const string MensagemCepInvalido = "O CEP tem 8 números. Confira e digite de novo.";
    public const string MensagemCepNaoEncontrado = "Não achamos esse CEP. Confira os números.";
    public const string MensagemCepIndisponivel = "Não conseguimos consultar o CEP agora. Tente de novo em alguns minutos.";

    /// <summary>Bairro, cidade e UF do CEP (para mostrar na tela antes de salvar).</summary>
    [HttpGet("{cep}")]
    [ProducesResponseType<EnderecoCepResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Consultar(string cep, CancellationToken ct)
    {
        var normalizado = CepUtil.Normalizar(cep);
        if (normalizado is null)
            return BadRequest(new MensagemResponse(MensagemCepInvalido));

        var resultado = await consultaCep.ConsultarAsync(normalizado, ct);
        return resultado switch
        {
            { Status: StatusConsultaCep.Encontrado, Endereco: { } e } => Ok(EnderecoCepResponse.De(e)),
            { Status: StatusConsultaCep.NaoEncontrado } => NotFound(new MensagemResponse(MensagemCepNaoEncontrado)),
            _ => StatusCode(StatusCodes.Status503ServiceUnavailable, new MensagemResponse(MensagemCepIndisponivel)),
        };
    }
}
