using FluentAssertions;
using SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Teste.Templates;

public class RelatorioSondagemConsolidadoDemograficoAppendMetaFiltrosTeste
{
    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_NaoDeveEmitirLinhas_QuandoTodosOsFiltrosSaoNulos()
    {
        var sb = new StringBuilder();

        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase.AppendLinhasMetaFiltrosOpcionais(
            sb,
            new RelatorioConsolidadoSondagemDto());

        sb.ToString().Should().BeEmpty();
    }

    [Theory]
    [InlineData(true, "Sim")]
    [InlineData(false, "Não")]
    public void AppendLinhasMetaFiltrosOpcionais_DeveEmitirSomentePap_ComValorInformado(bool valor, string esperadoNaCelula)
    {
        var sb = new StringBuilder();
        var dto = new RelatorioConsolidadoSondagemDto { Pap = valor };

        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase.AppendLinhasMetaFiltrosOpcionais(sb, dto);

        var html = NormalizarLf(sb.ToString());
        html.Should().Contain("<strong>PAP:</strong>");
        html.Should().Contain(esperadoNaCelula);
        html.Should().NotContain("AEE:");
        html.Should().NotContain("Deficiente:");
        Regex.Matches(html, "<tr>").Count.Should().Be(1);
        Regex.Matches(html, "<td></td>").Count.Should().Be(2);
    }

    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_DeveRespeitarOrdemPapAeeDeficientePortuguesSegundaLingua_QuandoTodosPreenchidos()
    {
        var sb = new StringBuilder();
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Pap = true,
            Aee = false,
            Deficiente = true,
            PossuiLinguaPortuguesaSegundaLingua = false
        };

        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase.AppendLinhasMetaFiltrosOpcionais(sb, dto);

        var html = NormalizarLf(sb.ToString());
        html.Should().Contain("<strong>PAP:</strong> Sim");
        html.Should().Contain("<strong>AEE:</strong> Não");
        html.Should().Contain("<strong>Deficiente:</strong> Sim");
        var rotuloPortuguesSegunda =
            $"<strong>{HttpUtility.HtmlEncode("Português como segunda língua")}:</strong> Não";
        html.Should().Contain(rotuloPortuguesSegunda);
        var indices = new[]
        {
            html.IndexOf("<strong>PAP:", StringComparison.Ordinal),
            html.IndexOf("<strong>AEE:", StringComparison.Ordinal),
            html.IndexOf("<strong>Deficiente:", StringComparison.Ordinal),
            html.IndexOf(rotuloPortuguesSegunda, StringComparison.Ordinal)
        };
        indices.Should().OnlyContain(i => i >= 0);
        indices[0].Should().BeLessThan(indices[1]);
        indices[1].Should().BeLessThan(indices[2]);
        indices[2].Should().BeLessThan(indices[3]);
    }

    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_DeveQuebrarEmDuasLinhasDeTresColunasEPreencherCelulasVazias_QuandoQuatroFiltros()
    {
        var sb = new StringBuilder();
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Pap = true,
            Aee = true,
            Deficiente = true,
            PossuiLinguaPortuguesaSegundaLingua = true
        };

        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase.AppendLinhasMetaFiltrosOpcionais(sb, dto);

        var html = NormalizarLf(sb.ToString());
        Regex.Matches(html, "<tr>").Count.Should().Be(2);
        Regex.Matches(html, "<td></td>").Count.Should().Be(2);
    }

    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_DeveEmitirUmaUnicaLinhaSemCelulasVazias_QuandoTresFiltros()
    {
        var sb = new StringBuilder();
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Pap = false,
            Aee = true,
            Deficiente = false
        };

        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase.AppendLinhasMetaFiltrosOpcionais(sb, dto);

        var html = NormalizarLf(sb.ToString());
        Regex.Matches(html, "<tr>").Count.Should().Be(1);
        html.Should().NotContain("<td></td>");
    }

    private static string NormalizarLf(string s) => s.ReplaceLineEndings("\n");
}
