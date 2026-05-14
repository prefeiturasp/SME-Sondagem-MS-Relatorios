using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
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
        hdr.Should().Contain("Gênero:</strong> Todos");
        hdr.Should().Contain("Raça:</strong> Todas");
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
