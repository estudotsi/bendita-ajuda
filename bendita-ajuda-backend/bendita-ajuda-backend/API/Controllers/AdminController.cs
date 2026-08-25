using bendita_ajuda_backend.API.DTOs.Servico;
using bendita_ajuda_backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace bendita_ajuda_backend.API.Controllers
{
    [ApiController]
    [Route("api/admin/servicos")]
    [Authorize]
    public class AdminServicoController : ControllerBase
    {
        private readonly ServicoService _servicoService;

        public AdminServicoController(ServicoService servicoService)
        {
            _servicoService = servicoService;
        }

        [HttpGet("pendentes")]
        public async Task<IActionResult> ListarPendentes()
        {
            var servicos = await _servicoService.ListarPendentesAsync();

            return Ok(servicos);
        }

        [HttpPut("{id}/aprovar")]
        public async Task<IActionResult> Aprovar(int id)
        {
            var aprovado = await _servicoService.AprovarAsync(id);

            if (!aprovado)
                return NotFound("Serviço não encontrado.");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var excluido = await _servicoService.ExcluirAsync(id);

            if (!excluido)
                return NotFound("Serviço não encontrado.");

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Criar(CriarServicoDto dto)
        {
            var criado = await _servicoService.CriarServicoAdminAsync(dto);

            if (!criado)
                return BadRequest("Serviço inválido ou já cadastrado.");

            return Created();
        }

        [HttpPut("{id}/corrigir-aprovar")]
        public async Task<IActionResult> CorrigirAprovar(int id,CorrigirAprovarServicoDto dto)
        {
            var aprovado = await _servicoService.CorrigirAprovarAsync(id, dto);

            if (!aprovado)
                return BadRequest("Serviço não encontrado, nome inválido ou já cadastrado.");

            return NoContent();
        }

        [HttpPut("{id}/substituir")]
        public async Task<IActionResult> Substituir(int id,SubstituirServicoDto dto)
        {
            var substituido = await _servicoService.SubstituirAsync(id, dto);

            if (!substituido)
                return BadRequest("Serviço pendente ou serviço correto inválido.");

            return NoContent();
        }
    }
}
