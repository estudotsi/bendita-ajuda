namespace bendita_ajuda_backend.API.DTOs.Agenda
{
    public class MesAgendaResponse
    {
        public int Mes { get; set; }

        public int Ano { get; set; }

        public string NomeMes { get; set; } = string.Empty;

        public List<DiaAgendaResponse> Dias { get; set; } = [];
    }
}
