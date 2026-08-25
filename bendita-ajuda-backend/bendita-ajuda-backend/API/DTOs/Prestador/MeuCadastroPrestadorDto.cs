namespace bendita_ajuda_backend.API.DTOs.Prestador
{
    public class MeuCadastroPrestadorDto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string TelefoneWhatsapp { get; set; } = string.Empty;

        public string Cep { get; set; } = string.Empty;
        public string Rua { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        public List<ServicoPrestadorDto> Servicos { get; set; } = [];
        public List<ServicoAguardandoAprovacaoDto> ServicosAguardandoAprovacao { get; set; } = [];
    }

    public class ServicoPrestadorDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class ServicoAguardandoAprovacaoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Status { get; set; } = "Aguardando aprovação";
    }
}
