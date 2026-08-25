namespace bendita_ajuda_backend.API.DTOs.Agenda
{
    public class HorarioAgendaResponse
    {
        public int Id { get; set; }

        public DateTime Inicio { get; set; }

        public DateTime Fim { get; set; }

        public string HoraInicio { get; set; } = string.Empty;

        public string HoraFim { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
