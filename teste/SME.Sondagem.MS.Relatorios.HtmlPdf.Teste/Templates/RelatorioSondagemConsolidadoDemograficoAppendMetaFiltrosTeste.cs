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
    private const string TagLinhaTr = "<tr>";
    private const string TagCelulaVazia = "<td></td>";
    private const string StrongPapFechado = "<strong>PAP:</strong>";
    private const string StrongAeeAberto = "<strong>AEE:";
    private const string StrongDeficienteAberto = "<strong>Deficiente:";
    private const string PrefixoAusenciaAee = "AEE:";
    private const string PrefixoAusenciaDeficiente = "Deficiente:";

    private static readonly string RotuloPortuguesSegundaNao =
        $"<strong>{HttpUtility.HtmlEncode("Português como segunda língua")}:</strong> Não";

    private static readonly TimeSpan LimiteRegex = TimeSpan.FromSeconds(1);

    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_NaoDeveEmitirLinhas_QuandoTodosOsFiltrosSaoNulos()
    {
        var html = ExecutarAppendNormalize(new RelatorioConsolidadoSondagemDto());

        html.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true, "Sim")]
    [InlineData(false, "Não")]
    public void AppendLinhasMetaFiltrosOpcionais_DeveEmitirSomentePap_ComValorInformado(bool valor, string esperadoNaCelula)
    {
        var dto = new RelatorioConsolidadoSondagemDto { Pap = valor };
        var html = ExecutarAppendNormalize(dto);

        html.Should().Contain(StrongPapFechado);
        html.Should().Contain(esperadoNaCelula);
        html.Should().NotContain(PrefixoAusenciaAee);
        html.Should().NotContain(PrefixoAusenciaDeficiente);
        ContarMatches(html, TagLinhaTr).Should().Be(1);
        ContarMatches(html, TagCelulaVazia).Should().Be(2);
    }

    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_DeveRespeitarOrdemPapAeeDeficientePortuguesSegundaLingua_QuandoTodosPreenchidos()
    {
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Pap = true,
            Aee = false,
            Deficiente = true,
            PossuiLinguaPortuguesaSegundaLingua = false
        };
        var html = ExecutarAppendNormalize(dto);

        html.Should().Contain($"{StrongPapFechado} Sim");
        html.Should().Contain($"{StrongAeeAberto}</strong> Não");
        html.Should().Contain($"{StrongDeficienteAberto}</strong> Sim");
        html.Should().Contain(RotuloPortuguesSegundaNao);

        var indices = new[]
        {
            html.IndexOf(StrongPapFechado, StringComparison.Ordinal),
            html.IndexOf(StrongAeeAberto, StringComparison.Ordinal),
            html.IndexOf(StrongDeficienteAberto, StringComparison.Ordinal),
            html.IndexOf(RotuloPortuguesSegundaNao, StringComparison.Ordinal)
        };
        indices.Should().OnlyContain(i => i >= 0);
        indices.Should().BeInAscendingOrder();
    }

    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_DeveQuebrarEmDuasLinhasDeTresColunasEPreencherCelulasVazias_QuandoQuatroFiltros()
    {
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Pap = true,
            Aee = true,
            Deficiente = true,
            PossuiLinguaPortuguesaSegundaLingua = true
        };
        var html = ExecutarAppendNormalize(dto);

        ContarMatches(html, TagLinhaTr).Should().Be(2);
        ContarMatches(html, TagCelulaVazia).Should().Be(2);
    }

    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_DeveEmitirUmaUnicaLinhaSemCelulasVazias_QuandoTresFiltros()
    {
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Pap = false,
            Aee = true,
            Deficiente = false
        };
        var html = ExecutarAppendNormalize(dto);

        ContarMatches(html, TagLinhaTr).Should().Be(1);
        html.Should().NotContain(TagCelulaVazia);
    }

    private static string ExecutarAppendNormalize(RelatorioConsolidadoSondagemDto dto)
    {
        var sb = new StringBuilder();
        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase.AppendLinhasMetaFiltrosOpcionais(sb, dto);
        return NormalizarLf(sb.ToString());
    }

    private static string NormalizarLf(string s) => s.ReplaceLineEndings("\n");

    private static int ContarMatches(string texto, string padraoLiteral) =>
        Regex.Count(texto, Regex.Escape(padraoLiteral), RegexOptions.None, LimiteRegex);
}
