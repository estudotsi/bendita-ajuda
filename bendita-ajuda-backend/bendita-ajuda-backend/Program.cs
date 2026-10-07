using System.Threading.RateLimiting;
using bendita_ajuda_backend.Controllers;
using bendita_ajuda_backend.Data;
using bendita_ajuda_backend.Data.Consultas;
using bendita_ajuda_backend.Dtos;
using bendita_ajuda_backend.Services;
using bendita_ajuda_backend.Services.Cep;
using bendita_ajuda_backend.Services.EnvioCodigo;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Erros de validação no mesmo formato dos demais: { mensagem }.
        options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(new MensagemResponse(PrimeiraMensagemDeErro(context.ModelState)));
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Banco de dados (MySQL). No servidor a conexão vem de "ConnectionStrings:DefaultConnection" (nome herdado do deploy antigo).
var connectionString = new[]
    {
        builder.Configuration.GetConnectionString("DefaultConnection"),
        builder.Configuration.GetConnectionString("Default"),
    }
    .FirstOrDefault(c => !string.IsNullOrWhiteSpace(c))
    ?? throw new InvalidOperationException("Configure a string de conexão \"ConnectionStrings:Default\".");

// Atrás do nginx: usa o IP real do cliente (rate limit) e o esquema https original.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(connectionString));

// Chaves que protegem o cookie ficam no banco: a sessão sobrevive a reinícios da API.
builder.Services.AddDataProtection()
    .SetApplicationName("bendita-ajuda")
    .PersistKeysToDbContext<AppDbContext>();

// Autenticação por cookie (sem Identity, sem JWT)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "bendita_sessao";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(90);
        options.SlidingExpiration = true;

        // É uma API: responde com o status em vez de redirecionar para uma página de login.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

// Limite por IP no envio de código (além do limite por número feito no AuthService).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, ct) => new ValueTask(
        context.HttpContext.Response.WriteAsJsonAsync(new MensagemResponse(AuthController.MensagemMuitosCodigos), ct));

    options.AddPolicy(AuthController.PoliticaEnviarCodigo, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
            }));
});

builder.Services.AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection(AuthOptions.Secao))
    .Validate(o => o.SegredoCodigo.Length >= 32, "Configure \"Auth:SegredoCodigo\" com pelo menos 32 caracteres.")
    .ValidateOnStart();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PrestadorService>();
builder.Services.AddScoped<SugestaoService>();

// Leituras com Dapper
builder.Services.AddScoped<ServicoConsultas>();
builder.Services.AddScoped<PrestadorConsultas>();
builder.Services.AddScoped<SugestaoConsultas>();

// CEP → bairro, cidade e UF (ViaCEP, gratuito)
builder.Services.AddHttpClient<IConsultaCep, ConsultaCepViaCep>(http =>
{
    http.BaseAddress = new Uri("https://viacep.com.br/");
    http.Timeout = TimeSpan.FromSeconds(10);
});

// Código de verificação por SMS (TrackMax). Sem "Sms:Token" configurado, só escreve o código no log.
var sms = builder.Configuration.GetSection(SmsOptions.Secao).Get<SmsOptions>() ?? new SmsOptions();
if (string.IsNullOrWhiteSpace(sms.Token))
{
    builder.Services.AddScoped<IEnvioCodigo, EnvioCodigoConsole>();
}
else
{
    builder.Services.AddHttpClient<IEnvioCodigo, EnvioCodigoSms>(http =>
    {
        http.BaseAddress = new Uri(sms.UrlBase);
        http.Timeout = TimeSpan.FromSeconds(15);
        http.DefaultRequestHeaders.Authorization = new("Bearer", sms.Token);
    });
}

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // tela de testes em /scalar
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string PrimeiraMensagemDeErro(ModelStateDictionary modelState)
{
    var erro = modelState
        .Where(campo => !campo.Key.StartsWith('$')) // erros de JSON malformado vêm em inglês
        .SelectMany(campo => campo.Value?.Errors ?? [])
        .Select(e => e.ErrorMessage)
        .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

    return erro ?? "Não entendemos os dados enviados. Confira e tente de novo.";
}
