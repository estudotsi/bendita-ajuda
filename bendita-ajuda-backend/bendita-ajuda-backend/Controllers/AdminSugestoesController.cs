using bendita_ajuda_backend.Data.Consultas;
using bendita_ajuda_backend.Data.Entidades;
using bendita_ajuda_backend.Dtos;
using bendita_ajuda_backend.Dtos.Admin;
using bendita_ajuda_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bendita_ajuda_backend.Controllers;

/// <summary>Tela do admin: serviços que os prestadores digitaram e não estão na lista.</summary>
[ApiController]
[Route("api/admin/sugestoes")]
[Authorize(Roles = nameof(Papel.Admin))]
public class AdminSugestoesController(SugestaoService sugestaoService, SugestaoConsultas consultas) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<SugestaoPendenteResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct) => Ok(await consultas.ListarPendentesAsync(ct));

    /// <summary>"É o mesmo que...": liga o prestador a um serviço existente.</summary>
    [HttpPost("{id:guid}/similar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Similar(Guid id, SugestaoSimilarRequest request, CancellationToken ct) =>
        Responder(await sugestaoService.MarcarComoSimilarAsync(id, request.ServicoId, ct));

    /// <summary>Cria um serviço novo a partir da sugestão.</summary>
    [HttpPost("{id:guid}/novo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Novo(Guid id, SugestaoNovoServicoRequest request, CancellationToken ct) =>
        Responder(await sugestaoService.CriarServicoAsync(id, request.Nome, request.NomePlural, ct));

    /// <summary>Recusa: serviço que não pode ser oferecido.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Recusar(Guid id, CancellationToken ct) =>
        Responder(await sugestaoService.RecusarAsync(id, ct));

    private IActionResult Responder(ResultadoSugestao resultado) => resultado switch
    {
        ResultadoSugestao.Resolvida => NoContent(),
        ResultadoSugestao.SugestaoNaoEncontrada =>
            NotFound(new MensagemResponse("Essa sugestão já foi resolvida. Atualize a página.")),
        ResultadoSugestao.ServicoNaoEncontrado =>
            NotFound(new MensagemResponse("Esse serviço não existe mais. Atualize a página.")),
        _ => Conflict(new MensagemResponse("Já existe um serviço com esse nome. Use \"É o mesmo serviço\".")),
    };
}
