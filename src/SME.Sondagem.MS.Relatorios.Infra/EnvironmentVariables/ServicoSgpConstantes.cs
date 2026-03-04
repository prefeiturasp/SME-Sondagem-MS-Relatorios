using System.Diagnostics.CodeAnalysis;

namespace SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;

[ExcludeFromCodeCoverage]
public static class ServicoSgpConstantes
{
    public const string SERVICO = "servicoSGP";
    public const string URL_REGISTRAR_SOLICITACAO_RELATORIO = "v1/solicitacao-relatorio/salvar";
    public const string URL_SOLICITACAO_RELATORIO = "v1/solicitacao-relatorio/obter-solicitacao-relatorio";
}
