using bendita_ajuda_backend.Domain;
using bendita_ajuda_backend.Domain.Entidades;
using bendita_ajuda_backend.Infra.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class SeedService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public SeedService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext context)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
    }

    public async Task CriarRoles()
    {
        string[] roles =
        [
            "Admin",
            "Operador",
            "Prestador",
            "Cliente"
        ];

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public async Task CriarAdmin()
    {
        var email = "admin@admin.com";
        var senha = "@Planaltina1";

        var admin = await _userManager.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Nome = "Administrador",
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            var resultado = await _userManager.CreateAsync(admin, senha);

            if (resultado.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }

    public async Task CriarServicos()
    {
        string[] servicos =
        [
            "Prestador",
            "Pedreiro",
            "Eletricista",
            "Encanador",
            "Pintor",
            "Diarista",
            "Jardineiro",
            "Chaveiro",
            "Marceneiro",
            "Técnico de Informática",
            "Manutenção de Ar-condicionado"
        ];

        foreach (var nome in servicos)
        {
            var existe = await _context.Servicos.AnyAsync(x => x.Nome == nome);

            if (!existe)
            {
                _context.Servicos.Add(new Servico
                {
                    Nome = nome,
                    Ativo = true
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}