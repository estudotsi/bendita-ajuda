namespace bendita_ajuda_backend.API.DTOs.Prestador
{
    public class PrestadorPorServicoDto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string TelefoneWhatsapp { get; set; } = string.Empty;

        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public double DistanciaKm { get; set; }
    }
}
