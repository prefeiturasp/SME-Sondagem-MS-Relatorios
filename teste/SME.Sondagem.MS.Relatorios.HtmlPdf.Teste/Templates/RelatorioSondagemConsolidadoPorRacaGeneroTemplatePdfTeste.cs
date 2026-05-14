using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using System.Web;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.HtmlPdf.Teste.Templates;

public class RelatorioSondagemConsolidadoPorRacaGeneroTemplatePdfTeste
{
    private readonly RelatorioSondagemConsolidadoPorRacaGeneroTemplatePdf _template = new();

    [Fact]
    public void GerarHtml_DeveEmitirSecaoPorGeneroComColunasDeRaca_QuandoDtoAninhado()
    {
        var dto = CriarDtoConsolidadoRacaGenero();

        var html = _template.GerarHtml(dto);
        var hdr = _template.GerarHtmlDocumentoCabecalhoWk(dto);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().NotContain("class=\"meta-table\"");
        hdr.Should().Contain("Por ra&#231;a e g&#234;nero"); // HtmlEncode no cabeçalho Wk
        hdr.Should().Contain("G&#234;nero:</strong> Todos");
        hdr.Should().Contain("Ra&#231;a:</strong> Todas");
        html.Should().Contain("Gênero: Feminino");
        html.Should().Contain("Gênero: Masculino");
        html.Should().Contain(">Branca<");
        html.Should().Contain(">Parda<");
        html.Should().Contain("PS");
        html.Should().Contain("cell-valor");
        html.Should().Contain("5");
        html.Should().Contain("12");
    }

    [Fact]
    public void GerarHtml_DeveIncluirLinhaTotalPorGenero_QuandoTotaisPorGeneroComRacasPreenchidos()
    {
        var dto = CriarDtoConsolidadoRacaGenero();

        var html = _template.GerarHtml(dto);

        html.Should().Contain(">Total<");
        html.Should().Contain("100");
    }

    [Fact]
    public void GerarHtml_DeveEmitirTodosOsGraficosAposTodasAsTabelas_QuandoHaVariasQuestoes()
    {
        var dto = CriarDtoConsolidadoRacaGenero();
        var questoes = new List<RelatorioConsolidadoQuestaoDto>();
        foreach (var q in dto.Questoes ?? [])
            questoes.Add(q);
        questoes.Add(CriarSegundaQuestaoParaOrdemGrafico());
        dto.Questoes = questoes;

        var html = _template.GerarHtml(dto);

        var idxPrimeiroGrafico = html.IndexOf("Gráfico da Sondagem", StringComparison.Ordinal);
        idxPrimeiroGrafico.Should().BeGreaterThan(0);

        var segundaQuestaoTh = $"<th>{HttpUtility.HtmlEncode("Segunda questão consolidada")}</th>";
        var idxSegundaQuestaoNaTabela = html.IndexOf(segundaQuestaoTh, StringComparison.Ordinal);
        idxSegundaQuestaoNaTabela.Should().BeGreaterThan(0);
        idxSegundaQuestaoNaTabela.Should().BeLessThan(idxPrimeiroGrafico);
    }

    private static RelatorioConsolidadoQuestaoDto CriarSegundaQuestaoParaOrdemGrafico()
    {
        return new RelatorioConsolidadoQuestaoDto
        {
            QuestaoId = 2,
            QuestaoNome = "Segunda questão consolidada",
            TotaisPorGeneroComRacas =
            [
                new RelatorioConsolidadoGeneroRacaDto
                {
                    Genero = "Feminino",
                    Racas =
                    [
                        new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 1, Percentual = 100 }
                    ]
                }
            ],
            Respostas =
            [
                new RelatorioConsolidadoRespostaDto
                {
                    Resposta = "X",
                    Ordem = 1,
                    Total = 1,
                    CorFundo = "#000000",
                    CorTexto = "#FFFFFF",
                    GenerosComRacas =
                    [
                        new RelatorioConsolidadoGeneroRacaDto
                        {
                            Genero = "Feminino",
                            Racas =
                            [
                                new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 1, Percentual = 100 }
                            ]
                        }
                    ]
                }
            ]
        };
    }

    private static RelatorioConsolidadoSondagemDto CriarDtoConsolidadoRacaGenero()
    {
        return new RelatorioConsolidadoSondagemDto
        {
            Titulo = "Escrita consolidado",
            Agrupamento = "Por raça e gênero",
            AnoLetivo = 2026,
            Modalidade = "EF",
            Dre = "Todas",
            UnidadeEducacional = "Todas",
            AnoTurma = "1° ANO",
            ComponenteCurricular = "Língua Portuguesa",
            Proficiencia = "Escrita",
            Bimestre = "Todos",
            Genero = "Todos",
            Raca = "Todas",
            RacasDisponiveis =
            [
                new RacaCor { Id = 1, Descricao = "Branca", CodigoEolRacaCor = 1 },
                new RacaCor { Id = 2, Descricao = "Parda", CodigoEolRacaCor = 2 }
            ],
            GenerosDisponiveis =
            [
                new GeneroSexo { Id = 1, Descricao = "Feminino", Sigla = "F" },
                new GeneroSexo { Id = 2, Descricao = "Masculino", Sigla = "M" }
            ],
            Questoes =
            [
                new RelatorioConsolidadoQuestaoDto
                {
                    QuestaoId = 1,
                    QuestaoNome = "Sistema de escrita",
                    TotaisPorGeneroComRacas =
                    [
                        new RelatorioConsolidadoGeneroRacaDto
                        {
                            Genero = "Feminino",
                            Racas =
                            [
                                new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 100, Percentual = 50 },
                                new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 100, Percentual = 50 }
                            ]
                        },
                        new RelatorioConsolidadoGeneroRacaDto
                        {
                            Genero = "Masculino",
                            Racas =
                            [
                                new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 80, Percentual = 40 },
                                new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 120, Percentual = 60 }
                            ]
                        }
                    ],
                    Respostas =
                    [
                        new RelatorioConsolidadoRespostaDto
                        {
                            Resposta = "PS",
                            Ordem = 1,
                            CorFundo = "#E02020",
                            CorTexto = "#FFFFFF",
                            GenerosComRacas =
                            [
                                new RelatorioConsolidadoGeneroRacaDto
                                {
                                    Genero = "Feminino",
                                    Racas =
                                    [
                                        new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 5, Percentual = 5 },
                                        new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 12, Percentual = 6 }
                                    ]
                                },
                                new RelatorioConsolidadoGeneroRacaDto
                                {
                                    Genero = "Masculino",
                                    Racas =
                                    [
                                        new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 3, Percentual = 3 },
                                        new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 8, Percentual = 4 }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        };
    }
}
