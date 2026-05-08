using Dapper;
using Npgsql;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;

namespace SME.Sondagem.MS.Relatorios.Dados.Repositorios;

public class RepositorioRacaCor : IRepositorioRacaCor
{
    private readonly ConnectionStringOptions _connectionStringOptions;

    public RepositorioRacaCor(ConnectionStringOptions connectionStringOptions)
    {
        _connectionStringOptions = connectionStringOptions;
    }

    public async Task<IEnumerable<RacaCor>> ObterTodosAsync()
    {
        const string sql = @"SELECT id AS Id,
                                    descricao AS Descricao,
                                    codigo_eol_raca_cor AS CodigoEolRacaCor,
                                    criado_em AS CriadoEm,
                                    criado_por AS CriadoPor,
                                    criado_rf AS CriadoRF,
                                    alterado_em AS AlteradoEm,
                                    alterado_por AS AlteradoPor,
                                    alterado_rf AS AlteradoRF,
                                    excluido AS Excluido
                               FROM raca_cor
                              WHERE NOT excluido
                              ORDER BY descricao";

        await using var conexao = new NpgsqlConnection(_connectionStringOptions.SGP_PostgresConsultas);
        return await conexao.QueryAsync<RacaCor>(sql);
    }
}
