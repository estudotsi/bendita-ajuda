using bendita_ajuda_backend.Data;
using bendita_ajuda_backend.Data.Consultas;
using bendita_ajuda_backend.Dtos.Busca;
using bendita_ajuda_backend.Dtos.Servicos;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace bendita_ajuda_backend.Services;

public enum ResultadoEnsinarPalavra
{
    Ensinada,
    BuscaNaoEncontrada,
    ServicoNaoEncontrado,
    PalavraInvalida,
}

/// <summary>
/// Busca de prestadores pela tela inicial e o aprendizado com o que não deu resultado:
/// texto que não entendemos vai para o admin ensinar; serviço sem prestador na cidade fica anotado.
/// </summary>
public class BuscaService(AppDbContext db, PrestadorConsultas consultas, ILogger<BuscaService> logger)
{
    private const int TamanhoMaximoPalavrasChave = 2000;
    private const int TamanhoMaximoTexto = 100;
    /// <summary>Palavras da frase usadas para procurar no nome do prestador.</summary>
    private const int MaximoPalavrasNome = 5;
    /// <summary>Texto que combina com muitos serviços não é anotado em todos.</summary>
    private const int MaximoServicosAnotados = 3;

    public async Task<BuscaPrestadoresResponse> BuscarAsync(
        BuscaPrestadoresRequest request, bool incluirWhatsapp, CancellationToken ct)
    {
        var bairro = Limpar(request.Bairro);
        var cidade = Limpar(request.Cidade);
        var uf = Limpar(request.Uf)?.ToUpperInvariant();

        var servicos = await db.Servicos.AsNoTracking().ToListAsync(ct);
        var idServico = Limpar(request.Servico);
        var texto = Texto.Normalizar(request.Texto);

        IReadOnlyList<Data.Entidades.Servico> encontrados = [];
        FiltroBusca filtro;
        if (idServico is not null)
        {
            encontrados = servicos.Where(s => s.Id == idServico).ToList();
            filtro = new FiltroBusca(encontrados.Select(s => s.Id).ToList(), [], bairro, cidade, uf, incluirWhatsapp);
        }
        else if (texto != "")
        {
            encontrados = BuscaPorTexto.ServicosDoTexto(servicos, texto);
            var palavrasNome = BuscaPorTexto.PalavrasUteis(texto).Take(MaximoPalavrasNome).ToList();
            filtro = new FiltroBusca(encontrados.Select(s => s.Id).ToList(), palavrasNome, bairro, cidade, uf, incluirWhatsapp);
        }
        else
        {
            filtro = new FiltroBusca(null, [], bairro, cidade, uf, incluirWhatsapp);
        }

        var prestadores = await consultas.BuscarAsync(filtro, ct);

        if (prestadores.Count == 0)
            await AnotarSemResultadoAsync(texto, idServico is null, encontrados.Select(s => s.Id), cidade, uf);

        return new BuscaPrestadoresResponse
        {
            ServicosEncontrados = encontrados
                .Select(s => new ServicoResponse { Id = s.Id, Nome = s.Nome, NomePlural = s.NomePlural })
                .ToList(),
            Prestadores = prestadores,
        };
    }

    /// <summary>
    /// "É o mesmo que...": a palavra vira palavra-chave do serviço. Apaga a busca e as outras
    /// anotadas que agora já combinam com ele (inclusive pedaços digitados, como "porta emp").
    /// </summary>
    public async Task<ResultadoEnsinarPalavra> EnsinarAsync(int buscaId, string servicoId, string palavra, CancellationToken ct)
    {
        var busca = await db.BuscasNaoEntendidas.FirstOrDefaultAsync(b => b.Id == buscaId, ct);
        if (busca is null)
            return ResultadoEnsinarPalavra.BuscaNaoEncontrada;

        var servico = await db.Servicos.FirstOrDefaultAsync(s => s.Id == servicoId, ct);
        if (servico is null)
            return ResultadoEnsinarPalavra.ServicoNaoEncontrado;

        var nova = Texto.Normalizar(palavra);
        if (nova.Length < 2)
            return ResultadoEnsinarPalavra.PalavraInvalida;

        var palavras = PalavrasChave.Acrescentar(servico.PalavrasChave, nova);
        if (palavras.Length > TamanhoMaximoPalavrasChave)
            return ResultadoEnsinarPalavra.PalavraInvalida;
        servico.PalavrasChave = palavras;

        var resolvidas = (await db.BuscasNaoEntendidas.ToListAsync(ct))
            .Where(b => b.Id == busca.Id || BuscaPorTexto.ServicosDoTexto([servico], b.Texto).Count > 0)
            .ToList();
        db.BuscasNaoEntendidas.RemoveRange(resolvidas);

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Palavra \"{Palavra}\" ensinada ao serviço {ServicoId} ({Total} buscas resolvidas).",
            nova, servico.Id, resolvidas.Count);
        return ResultadoEnsinarPalavra.Ensinada;
    }

    /// <summary>Busca sem sentido ou que não vale ensinar.</summary>
    public async Task<bool> ApagarNaoEntendidaAsync(int buscaId, CancellationToken ct) =>
        await db.BuscasNaoEntendidas.Where(b => b.Id == buscaId).ExecuteDeleteAsync(ct) > 0;

    /// <summary>
    /// Conta a busca que não achou ninguém. Falhar aqui não pode estragar a busca da pessoa,
    /// por isso o erro só vai para o log. INSERT ... ON DUPLICATE KEY UPDATE soma na mesma linha,
    /// sem corrida entre duas buscas iguais ao mesmo tempo.
    /// </summary>
    private async Task AnotarSemResultadoAsync(
        string texto, bool buscouPorTexto, IEnumerable<string> servicoIds, string? cidade, string? uf)
    {
        try
        {
            var conexao = db.Database.GetDbConnection();
            var agora = DateTime.UtcNow;
            var ids = servicoIds.Take(MaximoServicosAnotados).ToList();

            if (ids.Count > 0)
            {
                const string sql = """
                    INSERT INTO BuscasSemPrestador (ServicoId, Cidade, Uf, Quantidade, UltimaVez)
                    VALUES (@servicoId, @cidade, @uf, 1, @agora)
                    ON DUPLICATE KEY UPDATE Quantidade = Quantidade + 1, UltimaVez = @agora
                    """;
                foreach (var servicoId in ids)
                    await conexao.ExecuteAsync(sql, new { servicoId, cidade = cidade ?? "", uf = uf ?? "", agora });
            }
            else if (buscouPorTexto && BuscaPorTexto.PalavrasUteis(texto).Count > 0)
            {
                const string sql = """
                    INSERT INTO BuscasNaoEntendidas (Texto, Quantidade, PrimeiraVez, UltimaVez)
                    VALUES (@texto, 1, @agora, @agora)
                    ON DUPLICATE KEY UPDATE Quantidade = Quantidade + 1, UltimaVez = @agora
                    """;
                var limitado = texto.Length <= TamanhoMaximoTexto ? texto : texto[..TamanhoMaximoTexto].TrimEnd();
                await conexao.ExecuteAsync(sql, new { texto = limitado, agora });
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Não foi possível anotar a busca sem resultado \"{Texto}\".", texto);
        }
    }

    private static string? Limpar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
