using Dapper;
using Npgsql;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;

namespace SME.Sondagem.MS.Relatorios.Dados.Repositorios;

public class RepositorioBimestre : IRepositorioBimestre
{
    private readonly ConnectionStringOptions _connectionStringOptions;

    public RepositorioBimestre(ConnectionStringOptions connectionStringOptions)
    {
        _connectionStringOptions = connectionStringOptions;
    }

    public async Task<IEnumerable<Bimestre>> ObterTodosAsync()
    {
        const string sql = @"SELECT id AS Id,
                                    cod_bimestre_ensino_eol AS CodBimestreEnsinoEol,
                                    descricao AS Descricao,
                                    criado_em AS CriadoEm,
                                    criado_por AS CriadoPor,
                                    criado_rf AS CriadoRF,
                                    alterado_em AS AlteradoEm,
                                    alterado_por AS AlteradoPor,
                                    alterado_rf AS AlteradoRF,
                                    excluido AS Excluido
                               FROM bimestre
                              WHERE NOT excluido
                              ORDER BY id";

        await using var conexao = new NpgsqlConnection(_connectionStringOptions.SondagemConnection);
        return await conexao.QueryAsync<Bimestre>(sql);
    }
}
