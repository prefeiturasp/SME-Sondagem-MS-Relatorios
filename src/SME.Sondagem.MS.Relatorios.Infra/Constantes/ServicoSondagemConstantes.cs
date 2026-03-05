using System.Diagnostics.CodeAnalysis;

namespace SME.Sondagem.MS.Relatorios.Infra.Constantes;

[ExcludeFromCodeCoverage]
public static class ServicoSondagemConstantes
{
    public const string SERVICO = "servicoSondagem";
    public const string URL_REGISTRAR_SOLICITACAO_RELATORIO = "v1/solicitacao-relatorio/salvar";
    public const string URL_SOLICITACAO_RELATORIO = "relatorio-integracao/sondagem-por-turma";
}
