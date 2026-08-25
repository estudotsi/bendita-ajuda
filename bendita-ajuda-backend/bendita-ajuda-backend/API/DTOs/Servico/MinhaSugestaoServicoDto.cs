namespace bendita_ajuda_backend.API.DTOs.Servico
{
    public class MinhaSugestaoServicoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
