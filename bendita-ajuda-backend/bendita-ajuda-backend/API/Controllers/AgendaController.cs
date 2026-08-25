using bendita_ajuda_backend.API.DTOs.Agenda;
using bendita_ajuda_backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace bendita_ajuda_backend.API.Controllers
{
    [ApiController]
    [Route("api/agenda")]
    public class AgendaController : ControllerBase
    {
        private readonly AgendaPrestadorService _agendaPrestadorService;

        public AgendaController(AgendaPrestadorService agendaPrestadorService)
        {
            _agendaPrestadorService = agendaPrestadorService;
        }

        [Authorize]
        [HttpPost("prestador/gerar-horarios")]
        public async Task<IActionResult> GerarHorarios(
            [FromBody] GerarHorariosAgendaRequest request)
        {
            return await ExecutarComTratamentoAsync(async () =>
            {
                var usuarioId = ObterUsuarioId();

                var resultado = await _agendaPrestadorService.GerarHorariosAsync(
                    usuarioId,
                    request);

                return Ok(resultado);
            });
        }

        [Authorize]
        [HttpGet("prestador/minha-agenda")]
        public async Task<IActionResult> ObterMinhaAgenda(
            [FromQuery] DateTime? dataInicial,
            [FromQuery] DateTime? dataFinal)
        {
            return await ExecutarComTratamentoAsync(async () =>
            {
                var usuarioId = ObterUsuarioId();

                var resultado = await _agendaPrestadorService.ObterMinhaAgendaAsync(
                    usuarioId,
                    dataInicial,
                    dataFinal);

                return Ok(resultado);
            });
        }

        [HttpGet("prestador/{prestadorId:int}")]
        public async Task<IActionResult> ObterAgendaPublica(
            int prestadorId,
            [FromQuery] DateTime? dataInicial,
            [FromQuery] DateTime? dataFinal)
        {
            return await ExecutarComTratamentoAsync(async () =>
            {
                var resultado = await _agendaPrestadorService.ObterAgendaPublicaAsync(
                    prestadorId,
                    dataInicial,
                    dataFinal);

                return Ok(resultado);
            });
        }

        [Authorize]
        [HttpDelete("prestador/horarios/{horarioId:int}")]
        public async Task<IActionResult> CancelarHorario(int horarioId)
        {
            return await ExecutarComTratamentoAsync(async () =>
            {
                var usuarioId = ObterUsuarioId();

                await _agendaPrestadorService.CancelarHorarioAsync(
                    usuarioId,
                    horarioId);

                return NoContent();
            });
        }

        private string ObterUsuarioId()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new UnauthorizedAccessException("Usuário não autenticado.");

            return usuarioId;
        }

        private async Task<IActionResult> ExecutarComTratamentoAsync(
            Func<Task<IActionResult>> acao)
        {
            try
            {
                return await acao();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensagem = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    mensagem = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    mensagem = "Ocorreu um erro inesperado ao processar a solicitação."
                });
            }
        }
    }
}