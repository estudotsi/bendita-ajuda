using bendita_ajuda_backend.Dtos.Admin;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace bendita_ajuda_backend.Data.Consultas;

/// <summary>Leituras da tela de sugestões do admin (Dapper).</summary>
public class SugestaoConsultas(AppDbContext db)
{
    /// <summary>Todas as sugestões pendentes, as mais antigas primeiro.</summary>
    public async Task<IReadOnlyList<SugestaoPendenteResponse>> ListarPendentesAsync(CancellationToken ct)
    {
        const string sql = """
            SELECT ss.Id, ss.Descricao, ss.CriadoEm, u.Nome AS PrestadorNome, p.Cidade, p.Uf
            FROM ServicosSugeridos ss
            JOIN Prestadores p ON p.UsuarioId = ss.PrestadorId
            JOIN Usuarios u ON u.Id = p.UsuarioId
            ORDER BY ss.CriadoEm
            """;

        var conexao = db.Database.GetDbConnection();
        var sugestoes = await conexao.QueryAsync<SugestaoPendenteResponse>(new CommandDefinition(sql, cancellationToken: ct));
        return sugestoes.AsList();
    }
}
