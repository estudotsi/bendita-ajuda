using bendita_ajuda_backend.Data;
using bendita_ajuda_backend.Data.Entidades;
using bendita_ajuda_backend.Dtos.Prestadores;
using bendita_ajuda_backend.Services.Cep;
using Microsoft.EntityFrameworkCore;

namespace bendita_ajuda_backend.Services;

public enum ResultadoCadastroPrestador
{
    Cadastrado,
    JaEhPrestador,
    CepInvalido,
    CepNaoEncontrado,
    CepIndisponivel,
    ServicoInvalido,
    PrecisaServico,
}

/// <summary>Regras do "Quero oferecer meus serviços".</summary>
public class PrestadorService(AppDbContext db, IConsultaCep consultaCep, ILogger<PrestadorService> logger)
{
    /// <summary>
    /// Cria o cadastro de prestador numa operação só: serviços + local.
    /// Não existe prestador "pela metade": ou tem tudo, ou nada é salvo.
    /// </summary>
    public async Task<ResultadoCadastroPrestador> CadastrarAsync(
        Guid usuarioId, CadastrarPrestadorRequest request, CancellationToken ct)
    {
        if (await db.Prestadores.AnyAsync(p => p.UsuarioId == usuarioId, ct))
            return ResultadoCadastroPrestador.JaEhPrestador;

        var cep = CepUtil.Normalizar(request.Cep);
        if (cep is null)
            return ResultadoCadastroPrestador.CepInvalido;

        var idsEscolhidos = request.Servicos.Select(id => id.Trim()).Where(id => id != "").Distinct().ToList();
        var outroServico = request.OutroServico?.Trim();
        if (idsEscolhidos.Count == 0 && string.IsNullOrEmpty(outroServico))
            return ResultadoCadastroPrestador.PrecisaServico;

        // A lista é pequena: carregar toda facilita conferir os ids e procurar sinônimos.
        var todosServicos = await db.Servicos.ToListAsync(ct);

        var servicos = todosServicos.Where(s => idsEscolhidos.Contains(s.Id)).ToList();
        if (servicos.Count != idsEscolhidos.Count)
            return ResultadoCadastroPrestador.ServicoInvalido;

        // Cidade e UF sempre saem do CEP; o bairro a pessoa pode corrigir ou deixar vazio.
        var consulta = await consultaCep.ConsultarAsync(cep, ct);
        if (consulta.Status == StatusConsultaCep.NaoEncontrado)
            return ResultadoCadastroPrestador.CepNaoEncontrado;
        if (consulta.Endereco is not { } endereco)
            return ResultadoCadastroPrestador.CepIndisponivel;

        var bairro = request.Bairro is null ? endereco.Bairro : request.Bairro.Trim();

        var prestador = new Prestador
        {
            UsuarioId = usuarioId,
            Cep = cep,
            Bairro = string.IsNullOrEmpty(bairro) ? null : bairro,
            Cidade = endereco.Cidade,
            Uf = endereco.Uf,
            Visivel = true,
            CriadoEm = DateTime.UtcNow,
            Servicos = servicos,
        };

        if (!string.IsNullOrEmpty(outroServico))
        {
            // "boleiro" pode já ser palavra-chave de algum serviço: aí liga direto, sem passar pelo admin.
            var conhecido = ServicoComSinonimo(todosServicos, outroServico);
            if (conhecido is not null)
            {
                if (!prestador.Servicos.Contains(conhecido))
                    prestador.Servicos.Add(conhecido);
            }
            else
            {
                prestador.Sugestoes.Add(new ServicoSugerido
                {
                    Id = Guid.NewGuid(),
                    Descricao = outroServico,
                    CriadoEm = prestador.CriadoEm,
                });
            }
        }

        db.Prestadores.Add(prestador);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Usuário {UsuarioId} virou prestador ({Servicos} serviços, {Sugestoes} sugestões).",
            usuarioId, prestador.Servicos.Count, prestador.Sugestoes.Count);
        return ResultadoCadastroPrestador.Cadastrado;
    }

    /// <summary>Serviço cujo nome ou palavra-chave é igual ao texto (ignorando acentos e maiúsculas).</summary>
    public static Servico? ServicoComSinonimo(IEnumerable<Servico> servicos, string texto)
    {
        var alvo = Texto.Normalizar(texto);
        if (alvo == "")
            return null;

        return servicos.FirstOrDefault(s =>
            Texto.Normalizar(s.Nome) == alvo
            || Texto.Normalizar(s.NomePlural) == alvo
            || PalavrasChave.Separar(s.PalavrasChave).Contains(alvo));
    }
}
