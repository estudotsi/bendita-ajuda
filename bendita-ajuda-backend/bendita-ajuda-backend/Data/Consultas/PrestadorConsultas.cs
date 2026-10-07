using bendita_ajuda_backend.Dtos.Busca;
using bendita_ajuda_backend.Dtos.Prestadores;
using bendita_ajuda_backend.Dtos.Servicos;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace bendita_ajuda_backend.Data.Consultas;

/// <summary>Leituras do cadastro de prestador (Dapper).</summary>
public class PrestadorConsultas(AppDbContext db)
{
    /// <summary>Cadastro de prestador do usuário, ou null se ele ainda não é prestador.</summary>
    public async Task<MeuCadastroResponse?> MeuCadastroAsync(Guid usuarioId, CancellationToken ct)
    {
        const string sqlPrestador = """
            SELECT Cep, Bairro, Cidade, Uf, Visivel
            FROM Prestadores
            WHERE UsuarioId = @usuarioId
            """;

        const string sqlServicos = """
            SELECT s.Id, s.Nome, s.NomePlural
            FROM PrestadoresServicos ps
            JOIN Servicos s ON s.Id = ps.ServicoId
            WHERE ps.PrestadorId = @usuarioId
            ORDER BY s.Ordem, s.Nome
            """;

        const string sqlSugestoes = """
            SELECT Descricao
            FROM ServicosSugeridos
            WHERE PrestadorId = @usuarioId
            ORDER BY CriadoEm
            """;

        var conexao = db.Database.GetDbConnection();
        var parametros = new { usuarioId };

        var prestador = await conexao.QuerySingleOrDefaultAsync<MeuCadastroResponse>(
            new CommandDefinition(sqlPrestador, parametros, cancellationToken: ct));
        if (prestador is null)
            return null;

        var servicos = await conexao.QueryAsync<ServicoResponse>(
            new CommandDefinition(sqlServicos, parametros, cancellationToken: ct));
        var sugestoes = await conexao.QueryAsync<string>(
            new CommandDefinition(sqlSugestoes, parametros, cancellationToken: ct));

        return prestador with { Servicos = servicos.AsList(), SugestoesPendentes = sugestoes.AsList() };
    }

    /// <summary>
    /// Prestadores visíveis que fazem algum serviço. Os do mesmo bairro vêm primeiro,
    /// depois os da mesma cidade, depois o resto da UF.
    /// </summary>
    public async Task<IReadOnlyList<PrestadorResumoResponse>> BuscarAsync(FiltroBusca filtro, CancellationToken ct)
    {
        var parametros = new DynamicParameters(new
        {
            filtro.Bairro,
            filtro.Cidade,
            filtro.Uf,
            filtro.IncluirWhatsapp,
            limite = MaximoResultados,
        });

        // Com texto, vale achar pelo serviço OU pelo nome ("José" acha o José eletricista).
        var condicoes = new List<string>();
        if (filtro.ServicoIds is { Count: > 0 } ids)
        {
            condicoes.Add("EXISTS (SELECT 1 FROM PrestadoresServicos f WHERE f.PrestadorId = p.UsuarioId AND f.ServicoId IN @servicoIds)");
            parametros.Add("servicoIds", ids);
        }
        if (filtro.PalavrasNome.Count > 0)
        {
            // A collation do banco ignora acento: "jose%" acha "José". As palavras já vêm só com letras e números.
            var porPalavra = filtro.PalavrasNome.Select((palavra, i) =>
            {
                parametros.Add($"nomeInicio{i}", $"{palavra}%");
                parametros.Add($"nomeMeio{i}", $"% {palavra}%");
                return $"u.Nome LIKE @nomeInicio{i} OR u.Nome LIKE @nomeMeio{i}";
            });
            condicoes.Add($"({string.Join(" OR ", porPalavra)})");
        }

        // Buscou algo (serviço ou texto) e nada combinou: ninguém, e não "todos".
        if (filtro.ServicoIds is not null && condicoes.Count == 0)
            return [];

        var filtroTexto = condicoes.Count > 0 ? $"AND ({string.Join(" OR ", condicoes)})" : "";
        var filtroUf = filtro.Uf is null ? "" : "AND p.Uf = @Uf";

        var sql = $"""
            {SelectResumo}
            WHERE p.Visivel = 1
              AND EXISTS (SELECT 1 FROM PrestadoresServicos ps WHERE ps.PrestadorId = p.UsuarioId)
              {filtroUf}
              {filtroTexto}
            ORDER BY (p.Cidade = @Cidade AND p.Bairro = @Bairro) DESC, (p.Cidade = @Cidade) DESC, p.CriadoEm DESC
            LIMIT @limite
            """;

        var conexao = db.Database.GetDbConnection();
        var prestadores = (await conexao.QueryAsync<PrestadorResumoResponse>(
            new CommandDefinition(sql, parametros, cancellationToken: ct))).AsList();

        return await ComServicosAsync(prestadores, filtro.ServicoIds ?? [], ct);
    }

    /// <summary>Página do prestador. Null se não existe, está escondido ou ainda não tem serviço.</summary>
    public async Task<PrestadorResumoResponse?> PorIdAsync(Guid id, bool incluirWhatsapp, CancellationToken ct)
    {
        var sql = $"""
            {SelectResumo}
            WHERE p.UsuarioId = @id
              AND p.Visivel = 1
              AND EXISTS (SELECT 1 FROM PrestadoresServicos ps WHERE ps.PrestadorId = p.UsuarioId)
            """;

        var conexao = db.Database.GetDbConnection();
        var prestador = await conexao.QuerySingleOrDefaultAsync<PrestadorResumoResponse>(
            new CommandDefinition(sql, new { id, IncluirWhatsapp = incluirWhatsapp }, cancellationToken: ct));
        if (prestador is null)
            return null;

        return (await ComServicosAsync([prestador], [], ct))[0];
    }

    private const int MaximoResultados = 50;

    private const string SelectResumo = """
        SELECT p.UsuarioId AS Id, u.Nome, p.FotoUrl, p.Bio, p.Bairro, p.Cidade, p.Uf,
               CASE WHEN @IncluirWhatsapp THEN u.Celular END AS Whatsapp
        FROM Prestadores p
        JOIN Usuarios u ON u.Id = p.UsuarioId
        """;

    /// <summary>Preenche os serviços de cada prestador, com os que combinaram com a busca primeiro.</summary>
    private async Task<IReadOnlyList<PrestadorResumoResponse>> ComServicosAsync(
        List<PrestadorResumoResponse> prestadores, IReadOnlyList<string> servicosDaBusca, CancellationToken ct)
    {
        if (prestadores.Count == 0)
            return prestadores;

        const string sql = """
            SELECT ps.PrestadorId, s.Id, s.Nome
            FROM PrestadoresServicos ps
            JOIN Servicos s ON s.Id = ps.ServicoId
            WHERE ps.PrestadorId IN @ids
            ORDER BY s.Ordem, s.Nome
            """;

        var conexao = db.Database.GetDbConnection();
        var linhas = await conexao.QueryAsync<(Guid PrestadorId, string Id, string Nome)>(
            new CommandDefinition(sql, new { ids = prestadores.Select(p => p.Id).ToList() }, cancellationToken: ct));

        var porPrestador = linhas
            .GroupBy(l => l.PrestadorId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(l => servicosDaBusca.Contains(l.Id) ? 0 : 1)
                    .Select(l => new ServicoResumoResponse { Id = l.Id, Nome = l.Nome })
                    .ToList());

        return prestadores
            .Select(p => p with { Servicos = porPrestador.GetValueOrDefault(p.Id) ?? [] })
            .ToList();
    }
}

/// <summary>
/// O que a busca procura. <see cref="ServicoIds"/> null e sem palavras = todos;
/// com texto, acha quem faz um dos serviços OU tem uma das palavras no nome.
/// </summary>
public record FiltroBusca(
    IReadOnlyList<string>? ServicoIds,
    IReadOnlyList<string> PalavrasNome,
    string? Bairro,
    string? Cidade,
    string? Uf,
    bool IncluirWhatsapp);
