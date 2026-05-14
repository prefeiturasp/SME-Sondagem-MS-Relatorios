using System.Globalization;

namespace SME.Sondagem.MS.Relatorios.Infra.Helpers;

public static class DataImpressaoBrasiliaFormatador
{
    private static readonly TimeZoneInfo FusoHorarioBrasilia = ObterFusoHorarioBrasilia();
    private static readonly CultureInfo CulturaPtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string FormatarDataImpressaoFusoBrasilia(DateTime dataImpressao)
    {
        var emBrasilia = dataImpressao.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTime(dataImpressao, TimeZoneInfo.Utc, FusoHorarioBrasilia)
            : TimeZoneInfo.ConvertTime(dataImpressao, TimeZoneInfo.Local, FusoHorarioBrasilia);

        return emBrasilia.ToString("dd/MM/yyyy", CulturaPtBr);
    }

    private static TimeZoneInfo ObterFusoHorarioBrasilia()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
    }
}
