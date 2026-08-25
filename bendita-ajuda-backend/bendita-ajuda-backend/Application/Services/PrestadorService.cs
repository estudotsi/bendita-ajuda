using bendita_ajuda_backend.API.DTOs.Prestador;
using bendita_ajuda_backend.Domain;
using bendita_ajuda_backend.Domain.Entidades;
using bendita_ajuda_backend.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace bendita_ajuda_backend.Application.Services
{
    public class PrestadorService
    {
        private readonly AppDbContext _context;

        public PrestadorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StatusCadastroPrestadorDto?> ObterStatusCadastroAsync(string usuarioId)
        {
            var prestador = await _context.Prestadores
                .Include(x => x.PrestadorServicos)
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (prestador is null)
                return null;

            return new StatusCadastroPrestadorDto
            {
                TemEndereco = prestador.TemEnderecoCompleto(),
                TemServicos = prestador.TemServicosEscolhidos(),
                CadastroCompleto = prestador.CadastroCompleto()
            };
        }

        public async Task<bool> AtualizarEnderecoAsync(string usuarioId, AtualizarEnderecoPrestadorDto dto)
        {
            var prestador = await _context.Prestadores
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (prestador is null)
                return false;

            prestador.TelefoneWhatsapp = dto.TelefoneWhatsapp;
            prestador.Cep = dto.Cep;
            prestador.Rua = dto.Rua;
            prestador.Numero = dto.Numero;
            prestador.Bairro = dto.Bairro;
            prestador.Cidade = dto.Cidade;
            prestador.Estado = dto.Estado;
            prestador.Latitude = dto.Latitude;
            prestador.Longitude = dto.Longitude;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AtualizarServicosAsync(string usuarioId, AtualizarServicosPrestadorDto dto)
        {
            var prestador = await _context.Prestadores
                .Include(x => x.PrestadorServicos)
                    .ThenInclude(x => x.Servico)
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (prestador is null)
                return false;

            var vinculosAtivos = prestador.PrestadorServicos
                .Where(x => x.Servico.Ativo)
                .ToList();

            _context.PrestadorServicos.RemoveRange(vinculosAtivos);

            foreach (var servicoId in dto.ServicosIds.Distinct())
            {
                var servicoExisteAtivo = await _context.Servicos
                    .AnyAsync(x => x.Id == servicoId && x.Ativo);

                if (!servicoExisteAtivo)
                    continue;

                _context.PrestadorServicos.Add(new PrestadorServico
                {
                    PrestadorId = prestador.Id,
                    ServicoId = servicoId
                });
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<MeuCadastroPrestadorDto?> ObterMeuCadastroAsync(string usuarioId)
        {
            var prestador = await _context.Prestadores
                .Include(x => x.Usuario)
                .Include(x => x.PrestadorServicos)
                    .ThenInclude(x => x.Servico)
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (prestador is null)
                return null;

            return new MeuCadastroPrestadorDto
            {
                Id = prestador.Id,
                Nome = prestador.Usuario.Nome,
                Email = prestador.Usuario.Email ?? string.Empty,

                TelefoneWhatsapp = prestador.TelefoneWhatsapp,

                Cep = prestador.Cep,
                Rua = prestador.Rua,
                Numero = prestador.Numero,
                Bairro = prestador.Bairro,
                Cidade = prestador.Cidade,
                Estado = prestador.Estado,

                Servicos = prestador.PrestadorServicos
                    .Where(x => x.Servico.Ativo)
                    .Select(x => new ServicoPrestadorDto
                    {
                        Id = x.Servico.Id,
                        Nome = x.Servico.Nome
                    })
                    .ToList(),

                ServicosAguardandoAprovacao = prestador.PrestadorServicos
                    .Where(x => !x.Servico.Ativo)
                    .Select(x => new ServicoAguardandoAprovacaoDto
                    {
                        Id = x.Servico.Id,
                        Nome = x.Servico.Nome,
                        Status = "Aguardando aprovação"
                    })
                    .ToList()
            };
        }

        public async Task<List<PrestadorPorServicoDto>> ListarPorServicoAsync(int servicoId, double latitude, double longitude)
        {
            var prestadores = await _context.Prestadores
                .Include(x => x.Usuario)
                .Include(x => x.PrestadorServicos)
                    .ThenInclude(x => x.Servico)
                .Where(x =>
                    x.Ativo &&
                    x.Latitude.HasValue &&
                    x.Longitude.HasValue &&
                    x.PrestadorServicos.Any(ps =>
                        ps.ServicoId == servicoId &&
                        ps.Servico.Ativo))
                .ToListAsync();

            return prestadores
                .Select(x => new PrestadorPorServicoDto
                {
                    Id = x.Id,
                    Nome = x.Usuario.Nome,
                    TelefoneWhatsapp = x.TelefoneWhatsapp,
                    Cidade = x.Cidade,
                    Estado = x.Estado,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    DistanciaKm = CalcularDistanciaKm(
                        latitude,
                        longitude,
                        x.Latitude!.Value,
                        x.Longitude!.Value)
                })
                .OrderBy(x => x.DistanciaKm)
                .ToList();
        }

        private static double CalcularDistanciaKm(double latitudeOrigem, double longitudeOrigem, double latitudeDestino, double longitudeDestino)
        {
            const double raioTerraKm = 6371;

            double dLat = GrausParaRadianos(latitudeDestino - latitudeOrigem);
            double dLon = GrausParaRadianos(longitudeDestino - longitudeOrigem);

            double lat1 = GrausParaRadianos(latitudeOrigem);
            double lat2 = GrausParaRadianos(latitudeDestino);

            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) *
                Math.Cos(lat1) * Math.Cos(lat2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return Math.Round(raioTerraKm * c, 2);
        }

        private static double GrausParaRadianos(double graus)
        {
            return graus * Math.PI / 180;
        }
    }
}
