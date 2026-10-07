using bendita_ajuda_backend.Data.Consultas;
using bendita_ajuda_backend.Data.Entidades;
using bendita_ajuda_backend.Dtos;
using bendita_ajuda_backend.Dtos.Admin;
using bendita_ajuda_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bendita_ajuda_backend.Controllers;

/// <summary>Tela do admin: o que as pessoas procuraram e não acharam.</summary>
[ApiController]
[Route("api/admin/buscas")]
[Authorize(Roles = nameof(Papel.Admin))]
public class AdminBuscasController(BuscaService buscaService, BuscaConsultas consultas) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<BuscasSemResultadoResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct) => Ok(await consultas.ListarSemResultadoAsync(ct));

    /// <summary>"É o mesmo que...": a palavra vira palavra-chave do serviço.</summary>
    [HttpPost("nao-entendidas/{id:int}/ensinar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ensinar(int id, EnsinarPalavraRequest request, CancellationToken ct) =>
        await buscaService.EnsinarAsync(id, request.ServicoId, request.Palavra, ct) switch
        {
            ResultadoEnsinarPalavra.Ensinada => NoContent(),
            ResultadoEnsinarPalavra.BuscaNaoEncontrada =>
                NotFound(new MensagemResponse("Essa busca já foi resolvida. Atualize a página.")),
            ResultadoEnsinarPalavra.ServicoNaoEncontrado =>
                NotFound(new MensagemResponse("Esse serviço não existe mais. Atualize a página.")),
            _ => BadRequest(new MensagemResponse("Escreva uma palavra com pelo menos 2 letras.")),
        };

    /// <summary>Busca sem sentido ou que não vale ensinar.</summary>
    [HttpDelete("nao-entendidas/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Apagar(int id, CancellationToken ct) =>
        await buscaService.ApagarNaoEntendidaAsync(id, ct)
            ? NoContent()
            : NotFound(new MensagemResponse("Essa busca já foi resolvida. Atualize a página."));
}
