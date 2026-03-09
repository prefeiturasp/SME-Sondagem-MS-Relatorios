namespace SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;

public class ConnectionStringOptions
{
    public static string Secao => "ConnectionStrings";
    public string? SGP_Postgres { get; set; }
    public string? SGP_PostgresConsultas { get; set; }
}
