namespace SME.Sondagem.MS.Relatorios.Infra.Fila;

public static class RotasRabbit
{
    public static string RotaLogs => "ApplicationLog";
    public static string Log => "ApplicationLog";

    public const string RelatorioSondagemPorTurma = "sr.sondagem.relatorios.solicitados.sondagem.por.turma";
    public const string RotaRelatoriosProntosSgp = "sgp.relatorios.prontos.notificar";
    //public const string RotaRelatorioComErro = "sgp.relatorios.erro.notificar";
}
