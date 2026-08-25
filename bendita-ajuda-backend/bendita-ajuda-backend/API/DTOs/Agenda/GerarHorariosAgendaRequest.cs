namespace bendita_ajuda_backend.API.DTOs.Agenda
{
    public class GerarHorariosAgendaRequest
    {
        public DateTime Data { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFim { get; set; }
    }
}
