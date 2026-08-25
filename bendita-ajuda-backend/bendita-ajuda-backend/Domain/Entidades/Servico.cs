namespace bendita_ajuda_backend.Domain.Entidades
{
    public class Servico
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;

        public string? SugeridoPorUsuarioId { get; set; }
        public ApplicationUser? SugeridoPorUsuario { get; set; }

        public List<PrestadorServico> PrestadorServicos { get; set; } = [];
    }
}
