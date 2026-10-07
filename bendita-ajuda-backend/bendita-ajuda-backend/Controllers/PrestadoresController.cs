using System.Security.Claims;
using bendita_ajuda_backend.Data.Consultas;
using bendita_ajuda_backend.Dtos;
using bendita_ajuda_backend.Dtos.Prestadores;
using bendita_ajuda_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bendita_ajuda_backend.Controllers;

[ApiController]
[Route("api/prestadores")]
[Authorize]
public class PrestadoresController(PrestadorService prestadorService, PrestadorConsultas consultas) : ControllerBase
{
    /// <summary>Meu cadastro de prestador (404 se ainda não sou prestador).</summary>
    [HttpGet("eu")]
    [ProducesResponseType<MeuCadastroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MeuCadastro(CancellationToken ct)
    {
        var cadastro = await consultas.MeuCadastroAsync(UsuarioId(), ct);
        return cadastro is null ? NotFound() : Ok(cadastro);
    }

    /// <summary>Vira prestador: serviços + CEP, tudo de uma vez.</summary>
    [HttpPost("eu")]
    [ProducesResponseType<MeuCadastroResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Cadastrar(CadastrarPrestadorRequest request, CancellationToken ct)
    {
        var usuarioId = UsuarioId();
        var resultado = await prestadorService.CadastrarAsync(usuarioId, request, ct);

        return resultado switch
        {
            ResultadoCadastroPrestador.Cadastrado =>
                StatusCode(StatusCodes.Status201Created, await consultas.MeuCadastroAsync(usuarioId, ct)),
            ResultadoCadastroPrestador.JaEhPrestador =>
                Conflict(new MensagemResponse("Você já tem cadastro de prestador.")),
            ResultadoCadastroPrestador.CepInvalido =>
                BadRequest(new MensagemResponse(CepController.MensagemCepInvalido)),
            ResultadoCadastroPrestador.CepNaoEncontrado =>
                BadRequest(new MensagemResponse(CepController.MensagemCepNaoEncontrado)),
            ResultadoCadastroPrestador.CepIndisponivel =>
                StatusCode(StatusCodes.Status503ServiceUnavailable, new MensagemResponse(CepController.MensagemCepIndisponivel)),
            ResultadoCadastroPrestador.ServicoInvalido =>
                BadRequest(new MensagemResponse("Um dos serviços escolhidos não existe mais. Volte e escolha de novo.")),
            _ =>
                BadRequest(new MensagemResponse("Escolha pelo menos um serviço ou escreva o que você faz.")),
        };
    }

    private Guid UsuarioId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
