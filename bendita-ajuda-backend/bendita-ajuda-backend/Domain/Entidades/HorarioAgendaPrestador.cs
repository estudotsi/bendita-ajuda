using bendita_ajuda_backend.Domain.Enuns;

namespace bendita_ajuda_backend.Domain.Entidades
{
    public class HorarioAgendaPrestador
    {
        public int Id { get; set; }

        public int PrestadorId { get; set; }
        public Prestador Prestador { get; set; } = null!;

        public DateTime Inicio { get; set; }

        public DateTime Fim { get; set; }

        public StatusHorarioAgendaEnum Status { get; set; } = StatusHorarioAgendaEnum.Livre;

        public string? NomeCliente { get; set; }

        public string? TelefoneClienteWhatsapp { get; set; }

        public string? NomeServicoSolicitado { get; set; }

        public string? ObservacaoCliente { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public DateTime? ReservadoEm { get; set; }

        public DateTime? CanceladoEm { get; set; }

        public bool EstaLivre()
        {
            return Status == StatusHorarioAgendaEnum.Livre;
        }

        public void Reservar(
            string nomeCliente,
            string telefoneClienteWhatsapp,
            string? nomeServicoSolicitado,
            string? observacaoCliente)
        {
            if (!EstaLivre())
                throw new InvalidOperationException("Este horário não está disponível para reserva.");

            NomeCliente = nomeCliente;
            TelefoneClienteWhatsapp = telefoneClienteWhatsapp;
            NomeServicoSolicitado = nomeServicoSolicitado;
            ObservacaoCliente = observacaoCliente;
            Status = StatusHorarioAgendaEnum.Reservado;
            ReservadoEm = DateTime.UtcNow;
        }

        public void Cancelar()
        {
            if (Status == StatusHorarioAgendaEnum.Cancelado)
                return;

            Status = StatusHorarioAgendaEnum.Cancelado;
            CanceladoEm = DateTime.UtcNow;
        }

        public void Liberar()
        {
            NomeCliente = null;
            TelefoneClienteWhatsapp = null;
            NomeServicoSolicitado = null;
            ObservacaoCliente = null;
            Status = StatusHorarioAgendaEnum.Livre;
            ReservadoEm = null;
            CanceladoEm = null;
        }
    }
}
