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
    private const string PrefixoAusenciaAee = "AEE:";
    private const string PrefixoAusenciaDeficiente = "Deficiente:";

    private static readonly string StrongProgramasEAtendimentos =
        $"<strong>{HttpUtility.HtmlEncode("Programas e Atendimentos")}:</strong>";

    private static readonly string RotuloPortuguesSegundaNao =
        $"<strong>{HttpUtility.HtmlEncode("Português como segunda língua")}:</strong> Não";

    private static readonly TimeSpan LimiteRegex = TimeSpan.FromSeconds(1);

    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_NaoDeveEmitirLinhas_QuandoTodosOsFiltrosSaoNulosOuVazios()
    {
        var html = ExecutarAppendNormalize(new RelatorioConsolidadoSondagemDto());

        html.Should().BeEmpty();
    }

    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_DeveEmitirGeneroERaca_QuandoPreenchidos()
    {
        var dto = new RelatorioConsolidadoSondagemDto { Genero = "Feminino", Raca = "Parda" };
        var html = ExecutarAppendNormalize(dto);

        html.Should().Contain("<strong>G&#234;nero:</strong> Feminino");
        html.Should().Contain("<strong>Ra&#231;a:</strong> Parda");
        ContarMatches(html, TagLinhaTr).Should().Be(1);
        ContarMatches(html, TagCelulaVazia).Should().Be(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AppendLinhasMetaFiltrosOpcionais_DeveEmitirSomentePap_ComValorInformado(bool valor)
    {
        var dto = new RelatorioConsolidadoSondagemDto { Pap = valor };
        var html = ExecutarAppendNormalize(dto);

        var papFragmento = valor
            ? "PAP: Sim"
            : $"PAP: {HttpUtility.HtmlEncode("Não")}";
        html.Should().Contain(StrongProgramasEAtendimentos);
        html.Should().Contain(papFragmento);
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

        var idxProgramas = html.IndexOf(StrongProgramasEAtendimentos, StringComparison.Ordinal);
        var idxPap = html.IndexOf("PAP: Sim", StringComparison.Ordinal);
        var idxAee = html.IndexOf($"AEE: {HttpUtility.HtmlEncode("Não")}", StringComparison.Ordinal);
        var idxDef = html.IndexOf("Deficiente: Sim", StringComparison.Ordinal);
        var idxPt = html.IndexOf(RotuloPortuguesSegundaNao, StringComparison.Ordinal);

        new[] { idxProgramas, idxPap, idxAee, idxDef, idxPt }.Should().OnlyContain(i => i >= 0);
        idxProgramas.Should().BeLessThan(idxPap);
        idxPap.Should().BeLessThan(idxAee);
        idxAee.Should().BeLessThan(idxDef);
        idxDef.Should().BeLessThan(idxPt);
    }

    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_DeveQuebrarEmDuasLinhasDeTresColunasEPreencherCelulasVazias_QuandoQuatroFiltros()
    {
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Genero = "Feminino",
            Raca = "Parda",
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
    public void AppendLinhasMetaFiltrosOpcionais_DeveEmitirUmaUnicaLinhaSemCelulasVazias_QuandoGeneroRacaEProgramasPreenchidos()
    {
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Genero = "Masculino",
            Raca = "Parda",
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
