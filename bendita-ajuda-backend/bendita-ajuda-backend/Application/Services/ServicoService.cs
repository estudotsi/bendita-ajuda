using bendita_ajuda_backend.API.DTOs.Servico;
using bendita_ajuda_backend.Domain;
using bendita_ajuda_backend.Domain.Entidades;
using bendita_ajuda_backend.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace bendita_ajuda_backend.Application.Services
{
    public class ServicoService
    {
        private readonly AppDbContext _context;

        public ServicoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServicoDto>> ListarAtivosAsync()
        {
            return await _context.Servicos
                .Where(x => x.Ativo)
                .OrderBy(x => x.Nome)
                .Select(x => new ServicoDto
                {
                    Id = x.Id,
                    Nome = x.Nome
                })
                .ToListAsync();
        }

        public async Task<bool> SugerirServicoAsync(string usuarioId, SugerirServicoDto dto)
        {
            var nome = dto.Nome.Trim();

            if (string.IsNullOrWhiteSpace(nome))
                return false;

            var jaExiste = await _context.Servicos
                .AnyAsync(x => x.Nome.ToLower() == nome.ToLower());

            if (jaExiste)
                return false;

            var prestador = await _context.Prestadores
                .Include(x => x.PrestadorServicos)
                .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId);

            if (prestador is null)
                return false;

            var servico = new Servico
            {
                Nome = nome,
                Ativo = false,
                SugeridoPorUsuarioId = usuarioId
            };

            prestador.PrestadorServicos.Add(new PrestadorServico
            {
                Prestador = prestador,
                Servico = servico
            });

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<MinhaSugestaoServicoDto>> ListarMinhasSugestoesAsync(string usuarioId)
        {
            return await _context.Servicos
                .Where(x => x.SugeridoPorUsuarioId == usuarioId)
                .OrderBy(x => x.Nome)
                .Select(x => new MinhaSugestaoServicoDto
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    Ativo = x.Ativo,
                    Status = x.Ativo ? "Aprovado" : "Aguardando aprovação"
                })
                .ToListAsync();
        }

        public async Task<List<ServicoPendenteDto>> ListarPendentesAsync()
        {
            return await _context.Servicos
                .Include(x => x.SugeridoPorUsuario)
                .Where(x => !x.Ativo)
                .OrderBy(x => x.Nome)
                .Select(x => new ServicoPendenteDto
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    SugeridoPorUsuarioId = x.SugeridoPorUsuarioId,
                    SugeridoPorNome = x.SugeridoPorUsuario != null
                        ? x.SugeridoPorUsuario.Nome
                        : string.Empty,
                    SugeridoPorEmail = x.SugeridoPorUsuario != null
                        ? x.SugeridoPorUsuario.Email ?? string.Empty
                        : string.Empty,
                    Status = "Aguardando aprovação"
                })
                .ToListAsync();
        }

        public async Task<bool> AprovarAsync(int id)
        {
            var servico = await _context.Servicos
                .FirstOrDefaultAsync(x => x.Id == id);

            if (servico is null)
                return false;

            servico.Ativo = true;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExcluirAsync(int id)
        {
            var servico = await _context.Servicos
                .Include(x => x.PrestadorServicos)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (servico is null)
                return false;

            _context.PrestadorServicos.RemoveRange(servico.PrestadorServicos);
            _context.Servicos.Remove(servico);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CriarServicoAdminAsync(CriarServicoDto dto)
        {
            var nome = dto.Nome.Trim();

            if (string.IsNullOrWhiteSpace(nome))
                return false;

            var jaExiste = await _context.Servicos
                .AnyAsync(x => x.Nome.ToLower() == nome.ToLower());

            if (jaExiste)
                return false;

            var servico = new Servico
            {
                Nome = nome,
                Ativo = true
            };

            _context.Servicos.Add(servico);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CorrigirAprovarAsync(int id, CorrigirAprovarServicoDto dto)
        {
            var nome = dto.Nome.Trim();

            if (string.IsNullOrWhiteSpace(nome))
                return false;

            var servico = await _context.Servicos
                .FirstOrDefaultAsync(x => x.Id == id);

            if (servico is null)
                return false;

            var nomeJaExiste = await _context.Servicos
                .AnyAsync(x => x.Id != id && x.Nome.ToLower() == nome.ToLower());

            if (nomeJaExiste)
                return false;

            servico.Nome = nome;
            servico.Ativo = true;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SubstituirAsync(int id, SubstituirServicoDto dto)
        {
            if (id == dto.ServicoCorretoId)
                return false;

            var servicoPendente = await _context.Servicos
                .Include(x => x.PrestadorServicos)
                .FirstOrDefaultAsync(x => x.Id == id && !x.Ativo);

            if (servicoPendente is null)
                return false;

            var servicoCorretoExiste = await _context.Servicos
                .AnyAsync(x => x.Id == dto.ServicoCorretoId && x.Ativo);

            if (!servicoCorretoExiste)
                return false;

            var vinculosPendentes = servicoPendente.PrestadorServicos.ToList();

            foreach (var vinculo in vinculosPendentes)
            {
                var jaExisteVinculo = await _context.PrestadorServicos
                    .AnyAsync(x =>
                        x.PrestadorId == vinculo.PrestadorId &&
                        x.ServicoId == dto.ServicoCorretoId);

                if (!jaExisteVinculo)
                {
                    _context.PrestadorServicos.Add(new PrestadorServico
                    {
                        PrestadorId = vinculo.PrestadorId,
                        ServicoId = dto.ServicoCorretoId
                    });
                }
            }

            _context.PrestadorServicos.RemoveRange(vinculosPendentes);
            _context.Servicos.Remove(servicoPendente);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
