using bendita_ajuda_backend.Data;
using bendita_ajuda_backend.Data.Entidades;
using Microsoft.EntityFrameworkCore;

namespace bendita_ajuda_backend.Services;

public enum ResultadoSugestao
{
    Resolvida,
    SugestaoNaoEncontrada,
    ServicoNaoEncontrado,
    ServicoJaExiste,
}

/// <summary>
/// O admin resolvendo os serviços que os prestadores digitaram.
/// Cada ação vale para a sugestão e para todas as outras pendentes com o mesmo texto
/// (ex.: 5 prestadores escreveram "boleira": resolve uma vez só).
/// Tudo é gravado num único SaveChanges, ou seja, numa transação.
/// </summary>
public class SugestaoService(AppDbContext db, ILogger<SugestaoService> logger)
{
    private const int TamanhoMaximoPalavrasChave = 2000;
    private const int TamanhoMaximoIdServico = 50;

    /// <summary>"É o mesmo que...": liga os prestadores ao serviço e ensina o sinônimo.</summary>
    public async Task<ResultadoSugestao> MarcarComoSimilarAsync(Guid sugestaoId, string servicoId, CancellationToken ct)
    {
        var grupo = await CarregarGrupoAsync(sugestaoId, ct);
        if (grupo is null)
            return ResultadoSugestao.SugestaoNaoEncontrada;

        var servico = await db.Servicos.FirstOrDefaultAsync(s => s.Id == servicoId, ct);
        if (servico is null)
            return ResultadoSugestao.ServicoNaoEncontrado;

        var palavras = PalavrasChave.Acrescentar(servico.PalavrasChave, grupo[0].Descricao);
        if (palavras.Length <= TamanhoMaximoPalavrasChave)
            servico.PalavrasChave = palavras;

        LigarEApagar(grupo, servico);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Sugestão \"{Descricao}\" ligada ao serviço {ServicoId} ({Total} prestadores).",
            grupo[0].Descricao, servico.Id, grupo.Count);
        return ResultadoSugestao.Resolvida;
    }

    /// <summary>Cria um serviço novo e liga os prestadores a ele.</summary>
    public async Task<ResultadoSugestao> CriarServicoAsync(
        Guid sugestaoId, string nome, string nomePlural, CancellationToken ct)
    {
        var grupo = await CarregarGrupoAsync(sugestaoId, ct);
        if (grupo is null)
            return ResultadoSugestao.SugestaoNaoEncontrada;

        nome = Texto.PrimeiraMaiuscula(nome);
        nomePlural = Texto.PrimeiraMaiuscula(nomePlural);
        var id = Texto.Slug(nome, TamanhoMaximoIdServico);

        var todos = await db.Servicos.ToListAsync(ct);
        if (id == "" || todos.Any(s => s.Id == id || Texto.Normalizar(s.Nome) == Texto.Normalizar(nome)))
            return ResultadoSugestao.ServicoJaExiste;

        var servico = new Servico
        {
            Id = id,
            Nome = nome,
            NomePlural = nomePlural,
            PalavrasChave = PalavrasChave.Acrescentar(string.Empty, grupo[0].Descricao),
            Ordem = todos.Count == 0 ? 1 : todos.Max(s => s.Ordem) + 1,
        };
        db.Servicos.Add(servico);

        LigarEApagar(grupo, servico);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Serviço novo {ServicoId} criado a partir de \"{Descricao}\" ({Total} prestadores).",
            servico.Id, grupo[0].Descricao, grupo.Count);
        return ResultadoSugestao.Resolvida;
    }

    /// <summary>Serviço que não pode ser oferecido: só apaga as sugestões.</summary>
    public async Task<ResultadoSugestao> RecusarAsync(Guid sugestaoId, CancellationToken ct)
    {
        var grupo = await CarregarGrupoAsync(sugestaoId, ct);
        if (grupo is null)
            return ResultadoSugestao.SugestaoNaoEncontrada;

        db.ServicosSugeridos.RemoveRange(grupo);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Sugestão \"{Descricao}\" recusada ({Total} prestadores).", grupo[0].Descricao, grupo.Count);
        return ResultadoSugestao.Resolvida;
    }

    /// <summary>A sugestão escolhida (primeira da lista) + as pendentes com o mesmo texto.</summary>
    private async Task<List<ServicoSugerido>?> CarregarGrupoAsync(Guid sugestaoId, CancellationToken ct)
    {
        var escolhida = await db.ServicosSugeridos.FirstOrDefaultAsync(s => s.Id == sugestaoId, ct);
        if (escolhida is null)
            return null;

        var texto = Texto.Normalizar(escolhida.Descricao);
        var pendentes = await db.ServicosSugeridos
            .Include(s => s.Prestador).ThenInclude(p => p.Servicos)
            .ToListAsync(ct);

        return pendentes
            .Where(s => Texto.Normalizar(s.Descricao) == texto)
            .OrderBy(s => s.Id == sugestaoId ? 0 : 1)
            .ToList();
    }

    private void LigarEApagar(List<ServicoSugerido> grupo, Servico servico)
    {
        foreach (var sugestao in grupo)
        {
            if (!sugestao.Prestador.Servicos.Any(s => s.Id == servico.Id))
                sugestao.Prestador.Servicos.Add(servico);
        }
        db.ServicosSugeridos.RemoveRange(grupo);
    }
}
