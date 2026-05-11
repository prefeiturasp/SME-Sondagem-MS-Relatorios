namespace SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;

public class ConnectionStringOptions
{
    public static string Secao => "ConnectionStrings";
    public string? SondagemConnection { get; set; }
}
