namespace bendita_ajuda_backend.Domain.Entidades
{
    public class SolicitacaoServico
    {
        public int Id { get; set; }

        public int PrestadorId { get; set; }
        public Prestador Prestador { get; set; } = null!;

        public string NomeServico { get; set; } = string.Empty;
        public string? Descricao { get; set; }

        public string Status { get; set; } = "Pendente";

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AvaliadoEm { get; set; }

        public string? AvaliadoPorUsuarioId { get; set; }
        public ApplicationUser? AvaliadoPorUsuario { get; set; }
    }
}
