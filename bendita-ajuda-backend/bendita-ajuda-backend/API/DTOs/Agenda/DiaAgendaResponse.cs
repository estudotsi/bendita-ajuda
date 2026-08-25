namespace bendita_ajuda_backend.API.DTOs.Agenda
{
    public class DiaAgendaResponse
    {
        public DateTime Data { get; set; }

        public string DiaSemana { get; set; } = string.Empty;

        public List<HorarioAgendaResponse> Horarios { get; set; } = [];
    }
}
