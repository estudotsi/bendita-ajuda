using bendita_ajuda_backend.Dtos.Servicos;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace bendita_ajuda_backend.Data.Consultas;

/// <summary>Leituras da lista de serviços (Dapper).</summary>
public class ServicoConsultas(AppDbContext db)
{
    /// <summary>Todos os serviços, na ordem dos botões.</summary>
    public async Task<IReadOnlyList<ServicoResponse>> ListarAsync(CancellationToken ct)
    {
        const string sql = """
            SELECT Id, Nome, NomePlural
            FROM Servicos
            ORDER BY Ordem, Nome
            """;

        var conexao = db.Database.GetDbConnection();
        var servicos = await conexao.QueryAsync<ServicoResponse>(new CommandDefinition(sql, cancellationToken: ct));
        return servicos.AsList();
    }
}
