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

    public DbSet<Prestador> Prestadores => Set<Prestador>();

    public DbSet<Servico> Servicos => Set<Servico>();

    public DbSet<ServicoSugerido> ServicosSugeridos => Set<ServicoSugerido>();

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

        modelBuilder.Entity<Prestador>(prestador =>
        {
            prestador.ToTable("Prestadores");
            prestador.HasKey(p => p.UsuarioId);
            prestador.Property(p => p.Cep).HasMaxLength(8).IsRequired();
            prestador.Property(p => p.Bairro).HasMaxLength(100);
            prestador.Property(p => p.Cidade).HasMaxLength(100).IsRequired();
            prestador.Property(p => p.Uf).HasMaxLength(2).IsRequired();
            prestador.Property(p => p.Bio).HasMaxLength(500);
            prestador.Property(p => p.FotoUrl).HasMaxLength(500);

            prestador.HasOne(p => p.Usuario)
                .WithOne(u => u.Prestador)
                .HasForeignKey<Prestador>(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Muitos para muitos: o EF cria e mantém a tabela de ligação sozinho.
            prestador.HasMany(p => p.Servicos)
                .WithMany(s => s.Prestadores)
                .UsingEntity(
                    "PrestadoresServicos",
                    r => r.HasOne(typeof(Servico)).WithMany().HasForeignKey("ServicoId").OnDelete(DeleteBehavior.Restrict),
                    l => l.HasOne(typeof(Prestador)).WithMany().HasForeignKey("PrestadorId").OnDelete(DeleteBehavior.Cascade),
                    j => j.HasKey("PrestadorId", "ServicoId"));

            prestador.HasIndex(p => new { p.Uf, p.Cidade, p.Bairro });
        });

        modelBuilder.Entity<ServicoSugerido>(sugerido =>
        {
            sugerido.ToTable("ServicosSugeridos");
            sugerido.Property(s => s.Descricao).HasMaxLength(100).IsRequired();

            sugerido.HasOne(s => s.Prestador)
                .WithMany(p => p.Sugestoes)
                .HasForeignKey(s => s.PrestadorId)
                .OnDelete(DeleteBehavior.Cascade);

            sugerido.HasIndex(s => s.CriadoEm);
        });

        modelBuilder.Entity<Servico>(servico =>
        {
            servico.ToTable("Servicos");
            servico.Property(s => s.Id).HasMaxLength(50);
            servico.Property(s => s.Nome).HasMaxLength(100).IsRequired();
            servico.Property(s => s.NomePlural).HasMaxLength(100).IsRequired();
            servico.Property(s => s.PalavrasChave).HasMaxLength(2000).IsRequired();

            servico.HasIndex(s => s.Nome).IsUnique();
            servico.HasData(ServicosIniciais.Todos);
        });
    }
}
