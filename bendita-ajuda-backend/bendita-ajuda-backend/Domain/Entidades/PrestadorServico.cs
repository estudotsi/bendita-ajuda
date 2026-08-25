namespace bendita_ajuda_backend.Domain.Entidades
{
    public class PrestadorServico
    {
        public int PrestadorId { get; set; }
        public Prestador Prestador { get; set; } = null!;

        public int ServicoId { get; set; }
        public Servico Servico { get; set; } = null!;
    }
}
