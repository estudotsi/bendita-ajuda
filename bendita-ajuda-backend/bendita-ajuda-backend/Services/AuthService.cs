using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using bendita_ajuda_backend.Data;
using bendita_ajuda_backend.Data.Entidades;
using bendita_ajuda_backend.Services.EnvioCodigo;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace bendita_ajuda_backend.Services;

public enum ResultadoEnvioCodigo
{
    Enviado,
    CelularInvalido,
    LimiteExcedido,
    FalhaNoEnvio,
}

public enum StatusConfirmacao
{
    CodigoInvalido,
    PrecisaNome,
    Confirmado,
}

public record ResultadoConfirmacao(StatusConfirmacao Status, Usuario? Usuario = null);

/// <summary>Regras do "Entrar com celular": envio e confirmação do código.</summary>
public class AuthService(
    AppDbContext db,
    IEnvioCodigo envioCodigo,
    IOptions<AuthOptions> options,
    ILogger<AuthService> logger)
{
    private static readonly TimeSpan ValidadeCodigo = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan JanelaLimiteEnvio = TimeSpan.FromMinutes(15);
    private const int MaximoEnviosNaJanela = 3;
    private const int MaximoTentativas = 5;

    private readonly byte[] _segredo = Encoding.UTF8.GetBytes(options.Value.SegredoCodigo);

    public async Task<ResultadoEnvioCodigo> EnviarCodigoAsync(string celularDigitado, CancellationToken ct)
    {
        var celular = Celular.Normalizar(celularDigitado);
        if (celular is null)
            return ResultadoEnvioCodigo.CelularInvalido;

        var agora = DateTime.UtcNow;
        var inicioJanela = agora - JanelaLimiteEnvio;
        var enviosRecentes = await db.CodigosVerificacao
            .CountAsync(c => c.Celular == celular && c.CriadoEm > inicioJanela, ct);

        if (enviosRecentes >= MaximoEnviosNaJanela)
            return ResultadoEnvioCodigo.LimiteExcedido;

        var codigo = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        var registro = new CodigoVerificacao
        {
            Id = Guid.NewGuid(),
            Celular = celular,
            CodigoHash = CalcularHash(celular, codigo),
            ExpiraEm = agora + ValidadeCodigo,
            CriadoEm = agora,
        };
        db.CodigosVerificacao.Add(registro);
        await db.SaveChangesAsync(ct);

        if (!await envioCodigo.EnviarAsync(celular, codigo, ct))
        {
            // O código não chegou: descarta para não contar no limite de envios.
            db.CodigosVerificacao.Remove(registro);
            await db.SaveChangesAsync(CancellationToken.None);
            return ResultadoEnvioCodigo.FalhaNoEnvio;
        }

        return ResultadoEnvioCodigo.Enviado;
    }

    public async Task<ResultadoConfirmacao> ConfirmarCodigoAsync(
        string celularDigitado, string codigoDigitado, string? nome, CancellationToken ct)
    {
        var invalido = new ResultadoConfirmacao(StatusConfirmacao.CodigoInvalido);

        var celular = Celular.Normalizar(celularDigitado);
        if (celular is null)
            return invalido;

        var agora = DateTime.UtcNow;
        var codigo = await db.CodigosVerificacao
            .Where(c => c.Celular == celular
                && c.UsadoEm == null
                && c.ExpiraEm > agora
                && c.Tentativas < MaximoTentativas)
            .OrderByDescending(c => c.CriadoEm)
            .FirstOrDefaultAsync(ct);

        if (codigo is null)
            return invalido;

        if (!HashConfere(codigo.CodigoHash, CalcularHash(celular, codigoDigitado)))
        {
            // Na 5ª tentativa errada o código deixa de valer (filtro Tentativas < 5 acima).
            codigo.Tentativas++;
            await db.SaveChangesAsync(ct);
            return invalido;
        }

        var usuario = await db.Usuarios.Include(u => u.Prestador).FirstOrDefaultAsync(u => u.Celular == celular, ct);
        if (usuario is null)
        {
            var nomeLimpo = nome?.Trim();
            if (string.IsNullOrEmpty(nomeLimpo))
            {
                // O código continua valendo: o front reenvia o mesmo código junto com o nome.
                return new ResultadoConfirmacao(StatusConfirmacao.PrecisaNome);
            }

            usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = nomeLimpo,
                Celular = celular,
                CelularConfirmado = true,
                Papel = Papel.Cliente,
                CriadoEm = agora,
            };
            db.Usuarios.Add(usuario);
            logger.LogInformation("Novo usuário {UsuarioId} criado pelo celular.", usuario.Id);
        }
        else
        {
            usuario.CelularConfirmado = true;
        }

        codigo.UsadoEm = agora;
        await db.SaveChangesAsync(ct);

        return new ResultadoConfirmacao(StatusConfirmacao.Confirmado, usuario);
    }

    public async Task<Usuario?> BuscarUsuarioLogadoAsync(ClaimsPrincipal principal, CancellationToken ct)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(id, out var usuarioId))
            return null;

        return await db.Usuarios.AsNoTracking().Include(u => u.Prestador).FirstOrDefaultAsync(u => u.Id == usuarioId, ct);
    }

    /// <summary>Monta a identidade gravada no cookie de sessão.</summary>
    public static ClaimsPrincipal CriarPrincipal(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.Papel.ToString()),
        };
        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identidade);
    }

    /// <summary>HMAC-SHA256 de "celular:codigo" — o mesmo código gera hashes diferentes para números diferentes.</summary>
    private string CalcularHash(string celular, string codigo)
    {
        var hash = HMACSHA256.HashData(_segredo, Encoding.UTF8.GetBytes($"{celular}:{codigo}"));
        return Convert.ToHexString(hash);
    }

    private static bool HashConfere(string esperado, string calculado) =>
        CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(esperado), Encoding.ASCII.GetBytes(calculado));
}
