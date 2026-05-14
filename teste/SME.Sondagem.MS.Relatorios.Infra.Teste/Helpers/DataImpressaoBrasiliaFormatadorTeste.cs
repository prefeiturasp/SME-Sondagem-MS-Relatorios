using System.Globalization;
using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Infra.Helpers;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Helpers;

public class DataImpressaoBrasiliaFormatadorTeste
{
    /// <summary>
    /// Instantes em UTC com resultado de calendário em Brasília (UTC-3) conhecido.
    /// </summary>
    [Theory]
    [MemberData(nameof(CasosUtcComDataEsperada))]
    public void FormatarDataImpressaoFusoBrasilia_QuandoUtc_DeveExibirDataNoCalendarioDeBrasilia(
        int ano, int mes, int dia, int hora, int minuto, int segundo, string dataEsperadaDdMmYyyy)
    {
        var instanteUtc = new DateTime(ano, mes, dia, hora, minuto, segundo, DateTimeKind.Utc);

        var resultado = DataImpressaoBrasiliaFormatador.FormatarDataImpressaoFusoBrasilia(instanteUtc);

        resultado.Should().Be(dataEsperadaDdMmYyyy);
    }

    public static TheoryData<int, int, int, int, int, int, string> CasosUtcComDataEsperada { get; } = new()
    {
        // 03:00 UTC = 00:00 em Brasília (mesmo dia civil)
        { 2024, 6, 15, 3, 0, 0, "15/06/2024" },
        // 02:59:59 UTC = 23:59:59 do dia anterior em Brasília
        { 2024, 6, 15, 2, 59, 59, "14/06/2024" },
        // Virada de ano vista do Brasil
        { 2024, 1, 1, 3, 0, 0, "01/01/2024" },
        { 2024, 1, 1, 2, 59, 59, "31/12/2023" },
    };

    [Fact]
    public void FormatarDataImpressaoFusoBrasilia_QuandoLocal_DeveCoincidirComConversaoExplicitaParaBrasilia()
    {
        var dataLocal = new DateTime(2025, 3, 10, 14, 30, 0, DateTimeKind.Local);
        var tzBr = ObterFusoHorarioBrasiliaTeste();
        var esperado = TimeZoneInfo.ConvertTime(dataLocal, TimeZoneInfo.Local, tzBr)
            .ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));

        var resultado = DataImpressaoBrasiliaFormatador.FormatarDataImpressaoFusoBrasilia(dataLocal);

        resultado.Should().Be(esperado);
    }

    [Fact]
    public void FormatarDataImpressaoFusoBrasilia_QuandoUnspecified_DeveUsarMesmoComportamentoQueLocal()
    {
        var dataUnspecified = new DateTime(2025, 8, 20, 9, 0, 0, DateTimeKind.Unspecified);
        var tzBr = ObterFusoHorarioBrasiliaTeste();
        var esperado = TimeZoneInfo.ConvertTime(dataUnspecified, TimeZoneInfo.Local, tzBr)
            .ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR"));

        var resultado = DataImpressaoBrasiliaFormatador.FormatarDataImpressaoFusoBrasilia(dataUnspecified);

        resultado.Should().Be(esperado);
    }

    private static TimeZoneInfo ObterFusoHorarioBrasiliaTeste()
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
