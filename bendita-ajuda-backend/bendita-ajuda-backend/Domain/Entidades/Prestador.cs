namespace bendita_ajuda_backend.Domain.Entidades
{
    public class Prestador
    {
        public int Id { get; set; }

        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser Usuario { get; set; } = null!;

        public string TelefoneWhatsapp { get; set; } = string.Empty;

        public string Cep { get; set; } = string.Empty;
        public string Rua { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool Ativo { get; set; } = true;

        public List<PrestadorServico> PrestadorServicos { get; set; } = [];

        public List<HorarioAgendaPrestador> HorariosAgenda { get; set; } = [];

        public bool TemEnderecoCompleto()
        {
            return !string.IsNullOrWhiteSpace(Cep)
                && !string.IsNullOrWhiteSpace(Rua)
                && !string.IsNullOrWhiteSpace(Numero)
                && !string.IsNullOrWhiteSpace(Bairro)
                && !string.IsNullOrWhiteSpace(Cidade)
                && !string.IsNullOrWhiteSpace(Estado);
        }

        public bool TemServicosEscolhidos()
        {
            return PrestadorServicos.Any();
        }

        public bool CadastroCompleto()
        {
            return TemEnderecoCompleto() && TemServicosEscolhidos();
        }
    }
}