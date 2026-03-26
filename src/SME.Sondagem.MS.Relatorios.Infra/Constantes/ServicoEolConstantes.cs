using System.Diagnostics.CodeAnalysis;

namespace SME.Sondagem.MS.Relatorios.Infra.Constantes;

[ExcludeFromCodeCoverage]
public static class ServicoEolConstantes
{
    public const string SERVICO = "servicoEOL";
    public const string URL_BUSCAR_ESCOLAS = "escolas";
    public const string URL_BUSCAR_TURMA = "turmas/{0}/dados";
    public const string URL_BUSCAR_DADOS_USUARIO = "AutenticacaoSgp/{0}/dados";
}
