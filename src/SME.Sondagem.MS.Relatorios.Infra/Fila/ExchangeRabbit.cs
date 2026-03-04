namespace SME.Sondagem.MS.Relatorios.Infra.Fila;

public static class ExchangeRabbit
{
    public static string WorkerSondagemRelatorio => "sme.sgp.workers";
    public static string WorkerSondagemRelatorioDeadLetter => "sme.sgp.workers.deadletter";
    public static string Logs => "EnterpriseApplicationLog";
    public static int WorkerSondagemRelatorioDeadLetterTtl => 10 * 60 * 1000; /*10 Min * 60 Seg * 1000 milisegundos = 10 minutos em milisegundos*/
    public static int WorkerSondagemRelatorioDeadLetterDeadLetterTtl_3 => 3 * 60 * 1000; /*10 Min * 60 Seg * 1000 milisegundos = 10 minutos em milisegundos*/
}
