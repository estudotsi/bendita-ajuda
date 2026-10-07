using bendita_ajuda_backend.Dtos.Admin;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace bendita_ajuda_backend.Data.Consultas;

/// <summary>Leituras da tela de buscas sem resultado do admin (Dapper).</summary>
public class BuscaConsultas(AppDbContext db)
{
    private const int MaximoLinhas = 200;

    public async Task<BuscasSemResultadoResponse> ListarSemResultadoAsync(CancellationToken ct)
    {
        const string sqlNaoEntendidas = """
            SELECT Id, Texto, Quantidade, UltimaVez
            FROM BuscasNaoEntendidas
            ORDER BY Quantidade DESC, UltimaVez DESC
            LIMIT @limite
            """;

        // LEFT JOIN: se o serviço for apagado um dia, a linha continua aparecendo pelo id.
        const string sqlSemPrestador = """
            SELECT b.ServicoId, COALESCE(s.Nome, b.ServicoId) AS ServicoNome, b.Cidade, b.Uf, b.Quantidade, b.UltimaVez
            FROM BuscasSemPrestador b
            LEFT JOIN Servicos s ON s.Id = b.ServicoId
            ORDER BY b.Quantidade DESC, b.UltimaVez DESC
            LIMIT @limite
            """;

        var conexao = db.Database.GetDbConnection();
        var parametros = new { limite = MaximoLinhas };

        var naoEntendidas = await conexao.QueryAsync<BuscaNaoEntendidaResponse>(
            new CommandDefinition(sqlNaoEntendidas, parametros, cancellationToken: ct));
        var semPrestador = await conexao.QueryAsync<BuscaSemPrestadorResponse>(
            new CommandDefinition(sqlSemPrestador, parametros, cancellationToken: ct));

        return new BuscasSemResultadoResponse
        {
            NaoEntendidas = naoEntendidas.AsList(),
            SemPrestador = semPrestador.AsList(),
        };
    }
}
