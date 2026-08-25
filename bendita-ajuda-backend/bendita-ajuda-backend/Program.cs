using bendita_ajuda_backend.Application.Services;
using bendita_ajuda_backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddAppCors();
builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtConfiguration(builder.Configuration);

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<SeedService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<PrestadorService>();
builder.Services.AddScoped<ServicoService>();
builder.Services.AddScoped<AgendaPrestadorService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();
    await seedService.CriarRoles();
    await seedService.CriarAdmin();
    await seedService.CriarServicos();
}

app.Run();