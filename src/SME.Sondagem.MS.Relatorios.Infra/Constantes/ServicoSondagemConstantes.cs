using System.Diagnostics.CodeAnalysis;

namespace SME.Sondagem.MS.Relatorios.Infra.Constantes;

[ExcludeFromCodeCoverage]
public static class ServicoSondagemConstantes
{
    public const string SERVICO = "servicoSondagem";
    public const string URL_REGISTRAR_SOLICITACAO_RELATORIO = "v1/solicitacao-relatorio/salvar";
    public const string URL_SOLICITACAO_RELATORIO = "relatorio-integracao/sondagem-por-turma";
    public const string URL_SOLICITACAO_RELATORIO_CONSOLIDADO_POR_RACA = "relatorio-integracao/consolidado/raca";
    public const string URL_SOLICITACAO_RELATORIO_CONSOLIDADO_POR_GENERO = "relatorio-integracao/consolidado/genero";
    public const string URL_SOLICITACAO_RELATORIO_CONSOLIDADO_POR_ANO = "relatorio-integracao/consolidado/ano";
    public const string URL_SOLICITACAO_RELATORIO_CONSOLIDADO_POR_BIMESTRE = "relatorio-integracao/consolidado/bimestre";
    public const string URL_SOLICITACAO_RELATORIO_CONSOLIDADO_POR_RACA_GENERO = "relatorio-integracao/consolidado/raca-genero";
    public const string URL_SOLICITACAO_RELATORIO_CONSOLIDADO_POR_QUESTAO = "relatorio-integracao/consolidado/ano";
    public const string URL_PARAMETROS_SONDAGEM = "ParametroSondagemIntegracao/questionario/{0}";
    public const string URL_PROFICIENCIA = "relatorio-integracao/proficiencia/{0}";
}
