using Dapper;
using Npgsql;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;

namespace SME.Sondagem.MS.Relatorios.Dados.Repositorios;

public class RepositorioComponenteCurricular : IRepositorioComponenteCurricular
{
    private readonly ConnectionStringOptions _connectionStringOptions;

    public RepositorioComponenteCurricular(ConnectionStringOptions connectionStringOptions)
    {
        _connectionStringOptions = connectionStringOptions;
    }

    public async Task<ComponenteCurricular> ObterPorIdAsync(int id)
    {
        const string sql = @"SELECT id AS Id,
                                    nome AS Nome,
                                    ano AS Ano,
                                    modalidade AS Modalidade,
                                    codigo_eol AS CodigoEol,
                                    criado_em AS CriadoEm,
                                    criado_por AS CriadoPor,
                                    criado_rf AS CriadoRF,
                                    alterado_em AS AlteradoEm,
                                    alterado_por AS AlteradoPor,
                                    alterado_rf AS AlteradoRF,
                                    excluido AS Excluido
                               FROM componente_curricular
                              WHERE id = @id
                                AND NOT excluido";

        await using var conexao = new NpgsqlConnection(_connectionStringOptions.SondagemConnection);

        var result = await conexao.QueryFirstOrDefaultAsync<ComponenteCurricular>(sql, new { id });
        return result == null ? throw new InvalidOperationException($"ComponenteCurricular with id {id} not found.") : result;
    }
}
