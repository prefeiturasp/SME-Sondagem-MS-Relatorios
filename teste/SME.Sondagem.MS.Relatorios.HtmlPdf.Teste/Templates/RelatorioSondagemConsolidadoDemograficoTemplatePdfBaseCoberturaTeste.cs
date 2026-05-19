using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Teste.Templates;

/// <summary>
/// Cobre caminhos de <see cref="RelatorioSondagemConsolidadoDemograficoTemplatePdfBase"/> via subclasse concreta
/// (<see cref="RelatorioSondagemConsolidadoPorRacaTemplatePdf"/> + expositor auxiliar).
/// </summary>
public class RelatorioSondagemConsolidadoDemograficoTemplatePdfBaseCoberturaTeste
{
    [Fact]
    public void GerarHtml_QuestoesNulas_NaoEmitirBlocosDeRelatorioMasDocumentoCompleto()
    {
        var sut = new RelatorioSondagemConsolidadoPorRacaTemplatePdf();
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Titulo = "Só estrutura",
            Questoes = null!,
            RacasDisponiveis =
            [
                new RacaCor { Descricao = "Branca", CodigoEolRacaCor = 1 },
            ]
        };

        var html = sut.GerarHtml(dto);

        html.Should().StartWith("<!DOCTYPE html>").And.Contain("</html>");
        html.Should().NotContain("<div class=\"bloco-relatorio\">");
        html.Should().NotContain("<table class=\"main-table\">");
    }

    [Fact]
    public void GerarCssExtraSecaoTabelaH3_DeveInterpolarSnippetNoCabecalhoDeEstilo()
    {
        var sut = new TemplatePorRacaComCssExtra("/* extra-css-test */");
        var html = sut.GerarHtml(new RelatorioConsolidadoSondagemDto { Titulo = "X", Questoes = [] });

        html.Should().Contain("/* extra-css-test */");
    }

    [Fact]
    public void GerarCabecalhoWk_SemestreIdZeroOuInvalido_SeEjaEmitirTodos()
    {
        var sut = new RelatorioSondagemConsolidadoPorRacaTemplatePdf();
        foreach (var semestre in new[] { 0, 999 })
        {
            var dto = BaseDtoCabecalho();
            dto.ModalidadeId = (int)Modalidade.EJA;
            dto.SemestreId = semestre;

            var wk = sut.GerarHtmlDocumentoCabecalhoWk(dto);

            wk.Should().Contain("Semestre").And.Contain(HttpUtility.HtmlEncode("Todos"));
        }
    }

    [Fact]
    public void GerarCabecalhoWk_EjaComSemestreInformado_DeveExibirNomeSemestreCurto()
    {
        var sut = new RelatorioSondagemConsolidadoPorRacaTemplatePdf();
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Titulo = "Título",
            ModalidadeId = (int)Modalidade.EJA,
            SemestreId = (int)Semestre.Segundo,
            AnoLetivo = 2026,
            DataImpressao = new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Utc)
        };

        var wk = sut.GerarHtmlDocumentoCabecalhoWk(dto);

        wk.Should().Contain("Semestre");
        wk.Should().Contain(HttpUtility.HtmlEncode("2° Semestre"));
    }
    [Fact]
    public void GerarCabecalhoWk_ModalidadeFundamental_AnularAnoTurma_DeveEmitirTodos()
    {
        var sut = new RelatorioSondagemConsolidadoPorRacaTemplatePdf();
        var dto = BaseDtoCabecalho();
        dto.ModalidadeId = (int)Modalidade.Fundamental;
        dto.AnoTurma = "";

        var wk = sut.GerarHtmlDocumentoCabecalhoWk(dto);
        wk.Should().Contain("Ano:");
        wk.Should().Contain("Todos");
    }

    [Fact]
    public void GerarCabecalhoWk_OutraModalidade_DeveEmitirRotuloAnoTurmaCombinado()
    {
        var sut = new RelatorioSondagemConsolidadoPorRacaTemplatePdf();
        var dto = BaseDtoCabecalho();
        dto.ModalidadeId = (int)Modalidade.Medio;
        dto.AnoTurma = "2º série";

        var wk = sut.GerarHtmlDocumentoCabecalhoWk(dto);

        wk.Should().Contain("Ano / Turma:");
        wk.Should().Contain(HttpUtility.HtmlEncode("2º série"));
    }

    [Fact]
    public void GerarCabecalhoWk_TituloComCaracteresEspeciais_DeveHtmlEncodar()
    {
        var sut = new RelatorioSondagemConsolidadoPorRacaTemplatePdf();
        var dto = BaseDtoCabecalho();
        dto.Titulo = "A&B";

        sut.GerarHtmlDocumentoCabecalhoWk(dto).Should().Contain(HttpUtility.HtmlEncode("A&B"));
    }

    [Fact]
    public void MontarGraficoConsolidado_RespostasNulas_RetornaNulo()
    {
        TemplatePorRacaExpositor.ExporMontarGraficoConsolidado(new RelatorioConsolidadoQuestaoDto { Respostas = null }).Should().BeNull();
    }

    [Fact]
    public void MontarGraficoConsolidado_SomentesTotaisZero_RetornaNulo()
    {
        var modelo = TemplatePorRacaExpositor.ExporMontarGraficoConsolidado(new RelatorioConsolidadoQuestaoDto
        {
            QuestaoNome = "Questão sem barras úteis",
            Respostas =
            [
                new RelatorioConsolidadoRespostaDto { Total = 0, Ordem = 1 }
            ]
        });

        modelo.Should().BeNull();
    }

    [Fact]
    public void MontarGraficoConsolidado_UsaMarcadorQuandoLegendaRespostaAusenteOuEmBranco()
    {
        var modelo = TemplatePorRacaExpositor.ExporMontarGraficoConsolidado(new RelatorioConsolidadoQuestaoDto
        {
            QuestaoNome = "Nome Q",
            Respostas =
            [
                new RelatorioConsolidadoRespostaDto
                {
                    Total = 1,
                    Ordem = 1,
                    Resposta = "",
                    CorFundo = "",
                    CorTexto = "",
                },
            ]
        })!;

        modelo.Barras.Should().ContainSingle(b => b.Legenda == "—" && b.CorFundo == "#BFBFC2" && b.CorTexto == "#ffffff");
    }

    [Fact]
    public void FormatarCelulaValor_QuantidadeNaoPositivo_RetornaVazio()
    {
        TemplatePorRacaExpositor.ExporFormatarCelulaValor(0, 10).Should().Be("<span class=\"cell-vazio\">Vazio</span>");
        TemplatePorRacaExpositor.ExporFormatarCelulaValor(-1, 10).Should().Contain("cell-vazio");
    }

    [Fact]
    public void FormatarCelulaValor_QuantidadePositivo_FormatarPtBrComPercentual()
    {
        var html = TemplatePorRacaExpositor.ExporFormatarCelulaValor(12345, 8.752);
        html.Should().Contain("12.345"); // formato pt-BR
        html.Should().Contain("cell-valor").And.Contain("cell-percentual");
        html.Should().Contain("8,75%" /* N2 */);
    }

    [Fact]
    public void GerarBadgeNivel_SemCorFundo_UsarCelulaTexto()
    {
        var html = TemplatePorRacaExpositor.ExporGerarBadgeNivel(new RelatorioConsolidadoRespostaDto
        {
            Resposta = "'Texto'"
        });

        html.Should().Contain("cell-nivel-texto").And.Contain(HttpUtility.HtmlEncode("'Texto'"));
    }

    [Fact]
    public void GerarBadgeNivel_ComCores_EncodeFundoEtTextoFallbackBrancoPadrao()
    {
        var html = TemplatePorRacaExpositor.ExporGerarBadgeNivel(new RelatorioConsolidadoRespostaDto
        {
            Resposta = "Opção",
            CorFundo = "  #6933FF ",
            CorTexto = "",
        });

        html.Should().Contain("#6933FF").And.Contain("#ffffff").And.Contain("cell-nivel-badge");
    }

    [Fact]
    public void GerarPrefixoLinhaResposta_DeveMarcarLinhaSemPreenchimento_QuandoIgualAoRotuloCanonicalCaseInsensitive()
    {
        TemplatePorRacaExpositor.ExporGerarPrefixoLinha(new RelatorioConsolidadoRespostaDto
        {
            Resposta = "SEM PREENCHIMENTO",
        }).Should().Contain("row-sem-preenchimento");
    }

    [Fact]
    public void GerarPrefixoLinhaResposta_NaoDeveMarcarLinhaParaRespostaComum()
    {
        TemplatePorRacaExpositor.ExporGerarPrefixoLinha(new RelatorioConsolidadoRespostaDto
        {
            Resposta = "Resposta válida",
        }).Should().NotContain("row-sem-preenchimento");
    }
    [Fact]
    public void GerarGrafico_ModeleOuBarrasNulas_RetornaVazio()
    {
        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase.GerarGrafico(null).Should().BeEmpty();
        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase.GerarGrafico(new GraficoSondagemDto()).Should().BeEmpty();
    }

    [Fact]
    public void GerarGrafico_QuebraPaginaAntes_TriggerDivComPageBreakAntesDaTabelaPrincipal()
    {
        var modelo = GraficoMinimal();
        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase
            .GerarGrafico(modelo, quebrarPaginaAntes: true)
            .Should().Contain("page-break-before:");
    }

    [Fact]
    public void GerarGrafico_QuebraPaginaAntesFalse_NaoIncluirPageBreak()
    {
        var modelo = GraficoMinimal();
        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase
            .GerarGrafico(modelo, quebrarPaginaAntes: false)
            .Should().NotContain("page-break-before:");
    }

    [Fact]
    public void GerarGrafico_MuitasBarras_RedimensionaParaLarguraFixaFallback()
    {
        var barras = Enumerable.Range(1, 72).Select(i => new GraficoBarraDto
        {
            Legenda = $"B{i}",
            Quantidade = 1 + (i % 5),
            CorFundo = "#AAAAAA",
            CorTexto = "#000000",
        }).ToList();

        var modelo = new GraficoSondagemDto { Titulo = "Chart", Subtitulo = "Sub", Barras = barras };
        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase
            .GerarGrafico(modelo).Should().Contain("width:70px");
    }

    [Fact]
    public void GerarGrafico_ComBarraDeQuantidadeZero_NaoGerarBarrasInternasMasMantémContainer()
    {
        var modelo = new GraficoSondagemDto
        {
            Titulo = "Título",
            Subtitulo = "Sub",
            Barras =
            [
                new GraficoBarraDto { Quantidade = 0, Legenda = "Sem voto", CorFundo = "#EEE", CorTexto = "#000" },
            ],
        };

        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase
            .GerarGrafico(modelo).Should().Contain("Opções de respostas").And.NotContain("<span ");
    }

    [Fact]
    public void GerarHtml_DistribuirColunasDeRacasAprendidasSomenteDosTotais_QuandoOrdemPreferidoVazio()
    {
        var sut = new RelatorioSondagemConsolidadoPorRacaTemplatePdf();
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Titulo = "Colunas extrapoladas",
            RacasDisponiveis = [],
            Questoes =
            [
                new RelatorioConsolidadoQuestaoDto
                {
                    QuestaoNome = "Única questão",
                    TotalEstudantes = 2,
                    Respostas =
                    [
                        new RelatorioConsolidadoRespostaDto
                        {
                            Resposta = "Sim",
                            Ordem = 1,
                            Racas =
                            [
                                new RelatorioConsolidadoRacaDto { Raca = "Omega", Quantidade = 3, Percentual = 55 },
                            ]
                        },
                    ],
                    TotaisPorRaca =
                    [
                        new RelatorioConsolidadoRacaDto { Raca = "Beta", Quantidade = 12, Percentual = 65 },
                        new RelatorioConsolidadoRacaDto { Raca = "Alpha", Quantidade = 40, Percentual = 95 },
                        new RelatorioConsolidadoRacaDto { Raca = "Omega", Quantidade = 3, Percentual = 55 },
                    ],
                },
            ],
        };

        var html = sut.GerarHtml(dto);

        var idxOmega = html.IndexOf("Omega", StringComparison.Ordinal);
        idxOmega.Should().BeGreaterThan(-1); // garante inclusão mesmo fora dos Disponíveis
        html.Should().Contain("cell-vazio"); // valores ausentes no dicionário da linha ficam marcados como vazio
        html.Should().Contain(">Total</td>");
        html.Should().Contain("Beta").And.Contain("Alpha"); // aparece também na linha de totais ordenada
    }

    [Fact]
    public void GerarHtml_SemPossuirTotais_NaoGerarLinhaTotal()
    {
        var sut = new RelatorioSondagemConsolidadoPorRacaTemplatePdf();
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Titulo = "Totais opcionais",
            RacasDisponiveis = [new RacaCor { Descricao = "Branca", CodigoEolRacaCor = 1 }],
            Questoes =
            [
                new RelatorioConsolidadoQuestaoDto
                {
                    QuestaoNome = "Q",
                    TotaisPorRaca = [],
                    Respostas =
                    [
                        new RelatorioConsolidadoRespostaDto
                        {
                            Resposta = "M",
                            Ordem = 1,
                            Racas =
                            [
                                new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 2, Percentual = 100 },
                            ],
                        },
                    ],
                },
            ],
        };

        var htmlSemTotal = sut.GerarHtml(dto);

        htmlSemTotal.Should().Contain("tbody");
        htmlSemTotal.Should().NotContain("<tr class=\"row-total\">");
    }

    [Fact]
    public void GerarHtmlDocumentoCabecalhoVirtual_PodeSubstituirComportamento()
    {
        var sut = new CabecalhoWkMarcadoPorRaca();
        sut.GerarHtmlDocumentoCabecalhoWk(BaseDtoCabecalho()).Should().Be("<!-- marcador-wk-overridden -->");
    }

    [Fact]
    public void GerarHtml_ListaDeRespostasComEntradaNula_DeveGerarSomenteLinhasValidasNoCorpo()
    {
        var sut = new RelatorioSondagemConsolidadoPorRacaTemplatePdf();
        var dto = new RelatorioConsolidadoSondagemDto
        {
            Titulo = "Filtros nulos",
            RacasDisponiveis = [new RacaCor { Descricao = "Branca", CodigoEolRacaCor = 1 }],
            Questoes =
            [
                new RelatorioConsolidadoQuestaoDto
                {
                    QuestaoNome = "Única linha válida",
                    TotaisPorRaca = null,
                    Respostas =
                    [
                        null!,
                        new RelatorioConsolidadoRespostaDto
                        {
                            Resposta = "Sim",
                            Ordem = 1,
                            Racas =
                            [
                                new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 1, Percentual = 100 },
                            ],
                        },
                    ],
                },
            ],
        };

        var htmlGerado = sut.GerarHtml(dto);
        htmlGerado.Should().Contain("cell-nivel-texto").And.Contain(HttpUtility.HtmlEncode("Sim"));

        var idxTbodyOpen = htmlGerado.IndexOf("<tbody>", StringComparison.Ordinal);
        var idxTbodyFechamento = htmlGerado.IndexOf("</tbody>", idxTbodyOpen, StringComparison.Ordinal);
        var apenasCorpoTb = htmlGerado[(idxTbodyOpen + "<tbody>".Length)..idxTbodyFechamento];

        Regex.Count(apenasCorpoTb, "<tr", RegexOptions.None, TimeSpan.FromSeconds(2)).Should().Be(1);
    }

    [Fact]
    public void AppendLinhasMetaFiltrosOpcionais_EmitirPortuguesSim_QuandoTrue()
    {
        var sb = new StringBuilder();
        RelatorioSondagemConsolidadoDemograficoTemplatePdfBase.AppendLinhasMetaFiltrosOpcionais(
            sb,
            new RelatorioConsolidadoSondagemDto { PossuiLinguaPortuguesaSegundaLingua = true });

        var htmlSnippet = sb.ToString();
        htmlSnippet.Should().Contain("</strong> Sim</td>");
        htmlSnippet.Should().Contain("l&#237;ngua");
    }

    private static RelatorioConsolidadoSondagemDto BaseDtoCabecalho() =>
        new()
        {
            Titulo = "Titulo-X",
            Agrupamento = "Grupo X",
            AnoLetivo = 2026,
            Modalidade = "Ensino Fundamental",
            Dre = "DRE-1",
            UnidadeEducacional = "Escola Alfa",
            AnoTurma = "5º ano",
            ComponenteCurricular = "Matemática",
            Proficiencia = "Baixo",
            Bimestre = "B1",
            Usuario = "usuario.test",
            ModalidadeId = (int)Modalidade.Fundamental,
            SemestreId = 0,
            DataImpressao = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc),
        };

    private static GraficoSondagemDto GraficoMinimal() =>
        new()
        {
            Titulo = "Título",
            Subtitulo = "Sub",
            Barras =
            [
                new GraficoBarraDto { Legenda = "A", Quantidade = 120, CorFundo = "#111111", CorTexto = "#ffffff" },
            ],
        };

    private sealed class TemplatePorRacaComCssExtra : RelatorioSondagemConsolidadoPorRacaTemplatePdf
    {
        private readonly string _extraCss;
        internal TemplatePorRacaComCssExtra(string extraCss) => _extraCss = extraCss;
        protected override string CssExtraSecaoTabelaH3 => _extraCss;
    }

    private sealed class TemplatePorRacaExpositor : RelatorioSondagemConsolidadoPorRacaTemplatePdf
    {
        private static readonly TemplatePorRacaExpositor Instancia = new();
        public static string ExporFormatarCelulaValor(int quantidade, double percentual) =>
            FormatarCelulaValor(quantidade, percentual);

        public static string ExporGerarBadgeNivel(RelatorioConsolidadoRespostaDto resposta) =>
            GerarBadgeNivel(resposta);

        public static string ExporGerarPrefixoLinha(RelatorioConsolidadoRespostaDto resposta) =>
            GerarPrefixoLinhaRespostaComBadge(resposta);

        public static GraficoSondagemDto? ExporMontarGraficoConsolidado(RelatorioConsolidadoQuestaoDto questao) =>
            Instancia.MontarGraficoConsolidado(questao);
    }

    private sealed class CabecalhoWkMarcadoPorRaca : RelatorioSondagemConsolidadoPorRacaTemplatePdf
    {
        public override string GerarHtmlDocumentoCabecalhoWk(RelatorioConsolidadoSondagemDto dto) =>
            "<!-- marcador-wk-overridden -->";
    }
}
