using bendita_ajuda_backend.Data.Entidades;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace bendita_ajuda_backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    // Charset/collation (utf8mb4 + utf8mb4_0900_ai_ci, que ignora acentos e maiúsculas) ficam
    // como padrão do BANCO, definidos no CREATE DATABASE, e todas as tabelas herdam.
    // Não configurar no modelo: o MySql.EntityFrameworkCore 10 gera migrations que não compilam
    // quando HasCharSet/UseCollation são usados (referencia classes internas do provider).

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<CodigoVerificacao> CodigosVerificacao => Set<CodigoVerificacao>();

    /// <summary>Chaves do DataProtection: mantêm o cookie válido depois de reiniciar a API.</summary>
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(usuario =>
        {
            usuario.ToTable("Usuarios");
            usuario.Property(u => u.Nome).HasMaxLength(100).IsRequired();
            usuario.Property(u => u.Celular).HasMaxLength(20);
            usuario.Property(u => u.Email).HasMaxLength(254);
            usuario.Property(u => u.GoogleId).HasMaxLength(100);
            usuario.Property(u => u.Papel).HasConversion<int>();

            usuario.HasIndex(u => u.Celular).IsUnique();
            usuario.HasIndex(u => u.Email).IsUnique();
            usuario.HasIndex(u => u.GoogleId).IsUnique();
        });

        modelBuilder.Entity<CodigoVerificacao>(codigo =>
        {
            codigo.ToTable("CodigosVerificacao");
            codigo.Property(c => c.Celular).HasMaxLength(20).IsRequired();
            codigo.Property(c => c.CodigoHash).HasMaxLength(64).IsRequired();

            codigo.HasIndex(c => new { c.Celular, c.CriadoEm });
        });
    }
}
