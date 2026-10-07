using bendita_ajuda_backend.Data.Consultas;
using bendita_ajuda_backend.Dtos.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace bendita_ajuda_backend.Controllers;

[ApiController]
[Route("api/servicos")]
public class ServicosController(ServicoConsultas consultas) : ControllerBase
{
    /// <summary>Lista de serviços, na ordem dos botões.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ServicoResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct) => Ok(await consultas.ListarAsync(ct));
}
