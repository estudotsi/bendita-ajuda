using bendita_ajuda_backend.API.DTOs.Prestador;
using bendita_ajuda_backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace bendita_ajuda_backend.API.Controllers
{
    [ApiController]
    [Route("api/prestador")]
    public class PrestadorController : ControllerBase
    {
        private readonly PrestadorService _prestadorService;

        public PrestadorController(PrestadorService prestadorService)
        {
            _prestadorService = prestadorService;
        }

        [HttpGet("status-cadastro")]
        public async Task<IActionResult> ObterStatusCadastro()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var status = await _prestadorService.ObterStatusCadastroAsync(usuarioId);

            if (status is null)
                return NotFound("Prestador não encontrado.");

            return Ok(status);
        }

        [HttpPut("endereco")]
        public async Task<IActionResult> AtualizarEndereco(AtualizarEnderecoPrestadorDto dto)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var atualizado = await _prestadorService.AtualizarEnderecoAsync(usuarioId, dto);

            if (!atualizado)
                return NotFound("Prestador não encontrado.");

            return NoContent();
        }

        [HttpPut("servicos")]
        public async Task<IActionResult> AtualizarServicos(AtualizarServicosPrestadorDto dto)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var atualizado = await _prestadorService.AtualizarServicosAsync(usuarioId, dto);

            if (!atualizado)
                return NotFound("Prestador não encontrado.");

            return NoContent();
        }

        [HttpGet("meu-cadastro")]
        public async Task<IActionResult> ObterMeuCadastro()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var cadastro = await _prestadorService.ObterMeuCadastroAsync(usuarioId);

            if (cadastro is null)
                return NotFound("Prestador não encontrado.");

            return Ok(cadastro);
        }

        [HttpGet("por-servico/{servicoId}")]
        public async Task<IActionResult> ListarPorServico(int servicoId, [FromQuery] double latitude,[FromQuery] double longitude)
        {
            var prestadores = await _prestadorService.ListarPorServicoAsync(
                servicoId,
                latitude,
                longitude);

            return Ok(prestadores);
        }
    }
}
