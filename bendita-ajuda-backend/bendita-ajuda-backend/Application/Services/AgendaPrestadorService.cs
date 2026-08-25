using bendita_ajuda_backend.API.DTOs.Agenda;
using bendita_ajuda_backend.Domain.Entidades;
using bendita_ajuda_backend.Domain.Enuns;
using bendita_ajuda_backend.Infra.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace bendita_ajuda_backend.Application.Services
{
    public class AgendaPrestadorService
    {
        private readonly AppDbContext _context;

        public AgendaPrestadorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<HorarioAgendaResponse>> GerarHorariosAsync(
            string usuarioId,
            GerarHorariosAgendaRequest request)
        {
            var prestadorId = await ObterPrestadorIdPorUsuarioAsync(usuarioId);

            ValidarRequestGeracaoHorarios(request);

            var inicio = request.Data.Date.Add(request.HoraInicio);
            var fim = request.Data.Date.Add(request.HoraFim);

            ValidarPeriodoPermitidoParaCadastro(inicio);

            var existeConflito = await _context.HorariosAgendaPrestador
                .AnyAsync(h =>
                    h.PrestadorId == prestadorId &&
                    h.Status != StatusHorarioAgendaEnum.Cancelado &&
                    h.Inicio < fim &&
                    h.Fim > inicio);

            if (existeConflito)
                throw new InvalidOperationException("Já existem horários cadastrados nesse intervalo.");

            var horarios = GerarHorariosDeUmaEmUmaHora(
                prestadorId,
                inicio,
                fim);

            if (!horarios.Any())
                throw new InvalidOperationException("Nenhum horário foi gerado. Informe pelo menos 1 hora de intervalo.");

            _context.HorariosAgendaPrestador.AddRange(horarios);
            await _context.SaveChangesAsync();

            return horarios
                .OrderBy(h => h.Inicio)
                .Select(MapearHorario)
                .ToList();
        }

        public async Task<List<MesAgendaResponse>> ObterMinhaAgendaAsync(
            string usuarioId,
            DateTime? dataInicial,
            DateTime? dataFinal)
        {
            var prestadorId = await ObterPrestadorIdPorUsuarioAsync(usuarioId);

            var inicio = dataInicial?.Date ?? DateTime.Today;
            var fim = dataFinal?.Date ?? ObterLimiteMaximoAgenda();

            ValidarPeriodoConsulta(inicio, fim);

            var horarios = await _context.HorariosAgendaPrestador
                .AsNoTracking()
                .Where(h => h.PrestadorId == prestadorId)
                .Where(h => h.Status != StatusHorarioAgendaEnum.Cancelado)
                .Where(h => h.Inicio >= inicio)
                .Where(h => h.Inicio < fim.AddDays(1))
                .OrderBy(h => h.Inicio)
                .ToListAsync();

            return AgruparPorMes(horarios);
        }

        public async Task<List<MesAgendaResponse>> ObterAgendaPublicaAsync(
            int prestadorId,
            DateTime? dataInicial,
            DateTime? dataFinal)
        {
            var inicio = dataInicial?.Date ?? DateTime.Today;
            var fim = dataFinal?.Date ?? ObterLimiteMaximoAgenda();

            ValidarPeriodoConsulta(inicio, fim);

            var prestadorExiste = await _context.Prestadores
                .AsNoTracking()
                .AnyAsync(x => x.Id == prestadorId && x.Ativo);

            if (!prestadorExiste)
                throw new InvalidOperationException("Prestador não encontrado ou inativo.");

            var horarios = await _context.HorariosAgendaPrestador
                .AsNoTracking()
                .Where(h => h.PrestadorId == prestadorId)
                .Where(h => h.Status == StatusHorarioAgendaEnum.Livre)
                .Where(h => h.Inicio >= inicio)
                .Where(h => h.Inicio < fim.AddDays(1))
                .OrderBy(h => h.Inicio)
                .ToListAsync();

            return AgruparPorMes(horarios);
        }

        public async Task CancelarHorarioAsync(string usuarioId, int horarioId)
        {
            var prestadorId = await ObterPrestadorIdPorUsuarioAsync(usuarioId);

            var horario = await _context.HorariosAgendaPrestador
                .FirstOrDefaultAsync(h =>
                    h.Id == horarioId &&
                    h.PrestadorId == prestadorId);

            if (horario == null)
                throw new InvalidOperationException("Horário não encontrado.");

            if (horario.Inicio < DateTime.Now)
                throw new InvalidOperationException("Não é possível cancelar um horário que já passou.");

            horario.Cancelar();

            await _context.SaveChangesAsync();
        }

        private static void ValidarRequestGeracaoHorarios(GerarHorariosAgendaRequest request)
        {
            if (request.Data == default)
                throw new InvalidOperationException("Informe a data da agenda.");

            if (request.HoraFim <= request.HoraInicio)
                throw new InvalidOperationException("A hora final deve ser maior que a hora inicial.");

            if (request.HoraInicio.Minutes != 0 || request.HoraInicio.Seconds != 0)
                throw new InvalidOperationException("A hora inicial deve ser em hora cheia. Exemplo: 08:00, 09:00, 10:00.");

            if (request.HoraFim.Minutes != 0 || request.HoraFim.Seconds != 0)
                throw new InvalidOperationException("A hora final deve ser em hora cheia. Exemplo: 12:00, 13:00, 14:00.");
        }

        private static void ValidarPeriodoPermitidoParaCadastro(DateTime inicio)
        {
            if (inicio < DateTime.Now)
                throw new InvalidOperationException("Não é possível cadastrar horários no passado.");

            var limiteMaximo = ObterLimiteMaximoAgenda();

            if (inicio.Date > limiteMaximo.Date)
                throw new InvalidOperationException("Você só pode cadastrar horários para este mês e os próximos 2 meses.");
        }

        private static void ValidarPeriodoConsulta(DateTime inicio, DateTime fim)
        {
            if (fim < inicio)
                throw new InvalidOperationException("A data final deve ser maior ou igual à data inicial.");

            var limiteMaximo = ObterLimiteMaximoAgenda();

            if (fim.Date > limiteMaximo.Date)
                throw new InvalidOperationException("Você só pode consultar a agenda deste mês e dos próximos 2 meses.");
        }

        private static DateTime ObterLimiteMaximoAgenda()
        {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
                .AddMonths(3)
                .AddDays(-1);
        }

        private static List<HorarioAgendaPrestador> GerarHorariosDeUmaEmUmaHora(
            int prestadorId,
            DateTime inicio,
            DateTime fim)
        {
            var horarios = new List<HorarioAgendaPrestador>();

            var atual = inicio;

            while (atual < fim)
            {
                var proximo = atual.AddHours(1);

                if (proximo > fim)
                    break;

                horarios.Add(new HorarioAgendaPrestador
                {
                    PrestadorId = prestadorId,
                    Inicio = atual,
                    Fim = proximo,
                    Status = StatusHorarioAgendaEnum.Livre
                });

                atual = proximo;
            }

            return horarios;
        }

        private async Task<int> ObterPrestadorIdPorUsuarioAsync(string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new InvalidOperationException("Usuário não informado.");

            var prestador = await _context.Prestadores
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (prestador == null)
                throw new InvalidOperationException("Prestador não encontrado para o usuário logado.");

            return prestador.Id;
        }

        private static List<MesAgendaResponse> AgruparPorMes(List<HorarioAgendaPrestador> horarios)
        {
            var cultura = new CultureInfo("pt-BR");

            return horarios
                .GroupBy(h => new { h.Inicio.Year, h.Inicio.Month })
                .Select(mes => new MesAgendaResponse
                {
                    Ano = mes.Key.Year,
                    Mes = mes.Key.Month,
                    NomeMes = cultura.DateTimeFormat.GetMonthName(mes.Key.Month),
                    Dias = mes
                        .GroupBy(h => h.Inicio.Date)
                        .Select(dia => new DiaAgendaResponse
                        {
                            Data = dia.Key,
                            DiaSemana = cultura.DateTimeFormat.GetDayName(dia.Key.DayOfWeek),
                            Horarios = dia
                                .OrderBy(h => h.Inicio)
                                .Select(MapearHorario)
                                .ToList()
                        })
                        .OrderBy(d => d.Data)
                        .ToList()
                })
                .OrderBy(m => m.Ano)
                .ThenBy(m => m.Mes)
                .ToList();
        }

        private static HorarioAgendaResponse MapearHorario(HorarioAgendaPrestador horario)
        {
            return new HorarioAgendaResponse
            {
                Id = horario.Id,
                Inicio = horario.Inicio,
                Fim = horario.Fim,
                HoraInicio = horario.Inicio.ToString("HH:mm"),
                HoraFim = horario.Fim.ToString("HH:mm"),
                Status = horario.Status.ToString()
            };
        }
    }
}