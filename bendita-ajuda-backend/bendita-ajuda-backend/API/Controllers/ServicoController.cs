using System.Security.Claims;
using bendita_ajuda_backend.API.DTOs.Servico;
using bendita_ajuda_backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bendita_ajuda_backend.API.Controllers
{
    [ApiController]
    [Route("api/servicos")]
    public class ServicoController : ControllerBase
    {
        private readonly ServicoService _servicoService;

        public ServicoController(ServicoService servicoService)
        {
            _servicoService = servicoService;
        }

        [HttpGet("ativos")]
        public async Task<IActionResult> ListarAtivos()
        {
            var servicos = await _servicoService.ListarAtivosAsync();

            return Ok(servicos);
        }

        [HttpPost("sugestao")]
        public async Task<IActionResult> SugerirServico(SugerirServicoDto dto)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var criado = await _servicoService.SugerirServicoAsync(usuarioId, dto);

            if (!criado)
                return BadRequest("Serviço inválido ou já cadastrado.");

            return Created();
        }

        [HttpGet("minhas-sugestoes")]
        public async Task<IActionResult> ListarMinhasSugestoes()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var sugestoes = await _servicoService.ListarMinhasSugestoesAsync(usuarioId);

            return Ok(sugestoes);
        }
    }
}