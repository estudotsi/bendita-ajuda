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
}
