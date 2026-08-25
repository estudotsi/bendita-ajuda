using bendita_ajuda_backend.Domain.Entidades;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace bendita_ajuda_backend.Infra.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Prestador> Prestadores { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<PrestadorServico> PrestadorServicos { get; set; }
        public DbSet<SolicitacaoServico> SolicitacoesServico { get; set; }
        public DbSet<HorarioAgendaPrestador> HorariosAgendaPrestador { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PrestadorServico>()
                .HasKey(x => new { x.PrestadorId, x.ServicoId });

            builder.Entity<Prestador>()
                .HasOne(x => x.Usuario)
                .WithOne(x => x.Prestador)
                .HasForeignKey<Prestador>(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Prestador>()
                .HasIndex(x => x.UsuarioId)
                .IsUnique();

            builder.Entity<Servico>()
                .HasOne(x => x.SugeridoPorUsuario)
                .WithMany()
                .HasForeignKey(x => x.SugeridoPorUsuarioId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<PrestadorServico>()
                .HasOne(x => x.Prestador)
                .WithMany(x => x.PrestadorServicos)
                .HasForeignKey(x => x.PrestadorId);

            builder.Entity<PrestadorServico>()
                .HasOne(x => x.Servico)
                .WithMany(x => x.PrestadorServicos)
                .HasForeignKey(x => x.ServicoId);

            builder.Entity<SolicitacaoServico>()
                .HasOne(x => x.Prestador)
                .WithMany()
                .HasForeignKey(x => x.PrestadorId);

            builder.Entity<SolicitacaoServico>()
                .HasOne(x => x.AvaliadoPorUsuario)
                .WithMany()
                .HasForeignKey(x => x.AvaliadoPorUsuarioId)
                .IsRequired(false);

            builder.Entity<HorarioAgendaPrestador>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Inicio)
                    .IsRequired();

                entity.Property(x => x.Fim)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .IsRequired()
                    .HasConversion<int>();

                entity.Property(x => x.NomeCliente)
                    .HasMaxLength(150);

                entity.Property(x => x.TelefoneClienteWhatsapp)
                    .HasMaxLength(20);

                entity.Property(x => x.NomeServicoSolicitado)
                    .HasMaxLength(150);

                entity.Property(x => x.ObservacaoCliente)
                    .HasMaxLength(500);

                entity.Property(x => x.CriadoEm)
                    .IsRequired();

                entity.HasOne(x => x.Prestador)
                    .WithMany(x => x.HorariosAgenda)
                    .HasForeignKey(x => x.PrestadorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new { x.PrestadorId, x.Inicio });

                entity.HasIndex(x => new { x.PrestadorId, x.Inicio, x.Fim });
            });
        }

    }
}
