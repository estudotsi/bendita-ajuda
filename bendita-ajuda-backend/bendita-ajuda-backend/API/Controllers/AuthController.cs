using bendita_ajuda_backend.API.DTOs.Auth;
using bendita_ajuda_backend.Application.Services;
using bendita_ajuda_backend.Domain;
using bendita_ajuda_backend.Domain.Entidades;
using bendita_ajuda_backend.Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace bendita_ajuda_backend.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly EmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TokenService tokenService,
        EmailService emailService,
        AppDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var usuarioExistente = await _userManager.FindByEmailAsync(request.Email);

            if (usuarioExistente is not null)
                return BadRequest("E-mail já cadastrado.");

            var user = new ApplicationUser
            {
                Nome = request.Nome,
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = false,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Senha);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Cliente");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(roleResult.Errors);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var tokenEncoded = HttpUtility.UrlEncode(token);
            var emailEncoded = HttpUtility.UrlEncode(user.Email);

            var frontendUrl = _configuration["Frontend:Url"];

            var linkConfirmacao =
                $"{frontendUrl}/confirm-email?email={emailEncoded}&token={tokenEncoded}";

            var html = $@"
                <h2>Confirme seu e-mail</h2>
                <p>Olá, {user.Nome}.</p>
                <p>Clique no botão abaixo para confirmar sua conta:</p>
                <p>
                    <a href='{linkConfirmacao}'>
                        Confirmar e-mail
                    </a>
                </p>
            ";

            await _emailService.EnviarEmailAsync(
                user.Email!,
                "Confirme seu e-mail - Bendita Ajuda",
                html
            );

            return Ok(new
            {
                mensagem = "Conta criada com sucesso. Enviamos um e-mail de confirmação."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao criar conta: {ex.Message}");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
                return Unauthorized("E-mail ou senha inválidos.");

            if (!user.Ativo)
                return Unauthorized("Usuário inativo.");

            if (!user.EmailConfirmed)
                return Unauthorized("E-mail ainda não confirmado.");

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Senha,
                lockoutOnFailure: false
            );

            if (!result.Succeeded)
                return Unauthorized("E-mail ou senha inválidos.");

            var token = await _tokenService.GerarToken(user);

            var expiresInDays = _configuration.GetValue<int>("Jwt:ExpiresInDays");

            return Ok(new LoginResponse
            {
                AccessToken = token,
                ExpiresAt = DateTime.UtcNow.AddDays(expiresInDays)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao fazer login: {ex.Message}");
        }
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return BadRequest("Usuário não encontrado.");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                return BadRequest("Token inválido ou expirado.");

            return Ok("E-mail confirmado com sucesso.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao confirmar e-mail: {ex.Message}");
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
                return Ok("Se o e-mail existir, enviaremos as instruções.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var tokenEncoded = HttpUtility.UrlEncode(token);
            var emailEncoded = HttpUtility.UrlEncode(user.Email);

            var frontendUrl = _configuration["Frontend:Url"];

            var link =
                $"{frontendUrl}/reset-password?email={emailEncoded}&token={tokenEncoded}";

            var html = $@"
                <h2>Recuperação de senha</h2>
                <p>Olá, {user.Nome}.</p>
                <p>Clique no link abaixo para criar uma nova senha:</p>
                <a href='{link}'>Alterar senha</a>
            ";

            await _emailService.EnviarEmailAsync(
                user.Email!,
                "Recuperação de senha - Bendita Ajuda",
                html
            );

            return Ok("Se o e-mail existir, enviaremos as instruções.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao solicitar recuperação de senha: {ex.Message}");
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
                return BadRequest("Token inválido.");

            var result = await _userManager.ResetPasswordAsync(
                user,
                request.Token,
                request.NovaSenha
            );

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Senha alterada com sucesso.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao alterar senha: {ex.Message}");
        }
    }

    [HttpPost("register-prestador")]
    public async Task<IActionResult> RegisterPrestador(RegisterRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            var usuarioCriado = false;

            if (user is null)
            {
                user = new ApplicationUser
                {
                    Nome = request.Nome,
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = false,
                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Senha);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                usuarioCriado = true;
            }
            else if (!await _userManager.CheckPasswordAsync(user, request.Senha))
            {
                return Unauthorized("E-mail ou senha inválidos.");
            }

            if (await _userManager.IsInRoleAsync(user, "Prestador"))
                return BadRequest("Este usuário já é prestador.");

            var adicionarPrestadorResult = await _userManager.AddToRoleAsync(user, "Prestador");

            if (!adicionarPrestadorResult.Succeeded)
            {
                if (usuarioCriado)
                    await _userManager.DeleteAsync(user);

                return BadRequest(adicionarPrestadorResult.Errors);
            }

            if (await _userManager.IsInRoleAsync(user, "Cliente"))
            {
                var removerClienteResult = await _userManager.RemoveFromRoleAsync(user, "Cliente");

                if (!removerClienteResult.Succeeded)
                {
                    await _userManager.RemoveFromRoleAsync(user, "Prestador");

                    if (usuarioCriado)
                        await _userManager.DeleteAsync(user);

                    return BadRequest(removerClienteResult.Errors);
                }
            }

            var prestadorExistente = _context.Prestadores
                .FirstOrDefault(x => x.UsuarioId == user.Id);

            if (prestadorExistente is null)
            {
                var prestador = new Prestador
                {
                    UsuarioId = user.Id,
                    TelefoneWhatsapp = string.Empty,
                    Cep = string.Empty,
                    Rua = string.Empty,
                    Numero = string.Empty,
                    Bairro = string.Empty,
                    Cidade = string.Empty,
                    Estado = string.Empty,
                    Latitude = null,
                    Longitude = null,
                    Ativo = true
                };

                _context.Prestadores.Add(prestador);
                await _context.SaveChangesAsync();
            }

            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var tokenEncoded = HttpUtility.UrlEncode(token);
                var emailEncoded = HttpUtility.UrlEncode(user.Email);

                var frontendUrl = _configuration["Frontend:Url"];

                var linkConfirmacao =
                    $"{frontendUrl}/confirm-email?email={emailEncoded}&token={tokenEncoded}";

                var html = $@"
                <h2>Confirme seu e-mail</h2>
                <p>Olá, {user.Nome}.</p>
                <p>Clique no botão abaixo para confirmar sua conta de prestador:</p>
                <p>
                    <a href='{linkConfirmacao}'>
                        Confirmar e-mail
                    </a>
                </p>
            ";

                await _emailService.EnviarEmailAsync(
                    user.Email!,
                    "Confirme seu e-mail - Bendita Ajuda",
                    html
                );
            }

            return Ok(new
            {
                mensagem = user.EmailConfirmed
                    ? "Conta de prestador criada com sucesso."
                    : "Conta de prestador criada com sucesso. Enviamos um e-mail de confirmação."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao criar conta de prestador: {ex.Message}");
        }
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            mensagem = "Token válido",
            usuario = User.Identity?.Name
        });
    }
}
