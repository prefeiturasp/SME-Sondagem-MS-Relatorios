using Dapper;
using Npgsql;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;

namespace SME.Sondagem.MS.Relatorios.Dados.Repositorios;

public class RepositorioGeneroSexo : IRepositorioGeneroSexo
{
    private readonly ConnectionStringOptions _connectionStringOptions;

    public RepositorioGeneroSexo(ConnectionStringOptions connectionStringOptions)
    {
        _connectionStringOptions = connectionStringOptions;
    }

    public async Task<IEnumerable<GeneroSexo>> ObterTodosAsync()
    {
        const string sql = @"SELECT id AS Id,
                                    descricao AS Descricao,
                                    sigla AS Sigla,
                                    criado_em AS CriadoEm,
                                    criado_por AS CriadoPor,
                                    criado_rf AS CriadoRF,
                                    alterado_em AS AlteradoEm,
                                    alterado_por AS AlteradoPor,
                                    alterado_rf AS AlteradoRF,
                                    excluido AS Excluido
                               FROM genero_sexo
                              WHERE NOT excluido
                              ORDER BY descricao";

        await using var conexao = new NpgsqlConnection(_connectionStringOptions.SGP_PostgresConsultas);
        return await conexao.QueryAsync<GeneroSexo>(sql);
    }
}
