namespace bendita_ajuda_backend.API.DTOs.Servico
{
    public class ServicoPendenteDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        public string? SugeridoPorUsuarioId { get; set; }
        public string SugeridoPorNome { get; set; } = string.Empty;
        public string SugeridoPorEmail { get; set; } = string.Empty;

        public string Status { get; set; } = "Aguardando aprovação";
    }
}
