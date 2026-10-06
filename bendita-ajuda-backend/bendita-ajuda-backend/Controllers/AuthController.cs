using bendita_ajuda_backend.Dtos;
using bendita_ajuda_backend.Dtos.Auth;
using bendita_ajuda_backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace bendita_ajuda_backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    /// <summary>Nome da política do rate limiter (por IP) do envio de código.</summary>
    public const string PoliticaEnviarCodigo = "enviar-codigo";

    public const string MensagemMuitosCodigos = "Você pediu muitos códigos. Espere alguns minutos e tente de novo.";
    private const string MensagemCelularInvalido = "Esse número de celular não parece certo. Confira o DDD e o número.";
    private const string MensagemCodigoInvalido = "Código errado ou vencido. Confira ou peça um novo.";

    /// <summary>Envia um código de 6 números para o celular.</summary>
    [HttpPost("celular/enviar-codigo")]
    [EnableRateLimiting(PoliticaEnviarCodigo)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> EnviarCodigo(EnviarCodigoRequest request, CancellationToken ct)
    {
        var resultado = await authService.EnviarCodigoAsync(request.Celular, ct);

        return resultado switch
        {
            ResultadoEnvioCodigo.CelularInvalido => BadRequest(new MensagemResponse(MensagemCelularInvalido)),
            ResultadoEnvioCodigo.LimiteExcedido => StatusCode(StatusCodes.Status429TooManyRequests, new MensagemResponse(MensagemMuitosCodigos)),
            _ => NoContent(),
        };
    }

    /// <summary>Confere o código. Se o número ainda não tem conta, pede o nome (precisaNome: true).</summary>
    [HttpPost("celular/confirmar")]
    [ProducesResponseType<UsuarioLogadoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<PrecisaNomeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<MensagemResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Confirmar(ConfirmarCodigoRequest request, CancellationToken ct)
    {
        var resultado = await authService.ConfirmarCodigoAsync(request.Celular, request.Codigo, request.Nome, ct);

        switch (resultado.Status)
        {
            case StatusConfirmacao.PrecisaNome:
                return Ok(new PrecisaNomeResponse());

            case StatusConfirmacao.Confirmado when resultado.Usuario is not null:
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    AuthService.CriarPrincipal(resultado.Usuario),
                    new AuthenticationProperties { IsPersistent = true });
                return Ok(UsuarioLogadoResponse.De(resultado.Usuario));

            default:
                return BadRequest(new MensagemResponse(MensagemCodigoInvalido));
        }
    }

    /// <summary>Dados de quem está logado.</summary>
    [HttpGet("eu")]
    [Authorize]
    [ProducesResponseType<UsuarioLogadoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Eu(CancellationToken ct)
    {
        var usuario = await authService.BuscarUsuarioLogadoAsync(User, ct);
        if (usuario is null)
        {
            // Cookie válido, mas o usuário não existe mais.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Unauthorized();
        }

        return Ok(UsuarioLogadoResponse.De(usuario));
    }

    /// <summary>Sai da conta (apaga o cookie de sessão).</summary>
    [HttpPost("sair")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
