using Microsoft.AspNetCore.Identity;

namespace bendita_ajuda_backend.Domain.Entidades
{
    public class ApplicationUser : IdentityUser
    {
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public Prestador? Prestador { get; set; }
    }
}
