using FluentAssertions;
using Moq;
using SME.Sondagem.MS.Relatorios.Excel.Templates;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Excel.Teste.Templates;

public class RelatorioSondagemConsolidadoGenericoTemplateExcelTeste
{
    private readonly Mock<IServicoArmazenamentoMinio> _mockerServicoArmazenamentoMinio;
    private readonly RelatorioSondagemConsolidadoGenericoTemplateExcel _templateExcel;

    public RelatorioSondagemConsolidadoGenericoTemplateExcelTeste()
    {
        _mockerServicoArmazenamentoMinio = new Mock<IServicoArmazenamentoMinio>();
        _templateExcel = new RelatorioSondagemConsolidadoGenericoTemplateExcel(_mockerServicoArmazenamentoMinio.Object);
    }

    [Fact]
    public async Task GerarExcelEF_DeveGerarExcelComSucesso_ComDadosPorBimestre()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterDtoComBimestres(codigoCorrelacao);
        var linkDownload = "http://minio/relatorio-consolidado-generico.xlsx";

        ConfigurarMinioMock(codigoCorrelacao, linkDownload);

        // Act
        var result = await _templateExcel.GerarExcelEF(relatorioDto);

        // Assert
        result.Should().Be(linkDownload);
        _mockerServicoArmazenamentoMinio.Verify(s => s.UploadRelatorioAsync(
            It.IsAny<byte[]>(),
            It.Is<string>(n => n.Contains(codigoCorrelacao.ToString())),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        ), Times.Once);
    }

    [Fact]
    public async Task GerarExcelEF_DeveGerarExcel_QuandoQuestoesForemVazias()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterDtoBase(codigoCorrelacao);
        relatorioDto.Questoes = [];
        var linkDownload = "http://minio/relatorio-consolidado-generico.xlsx";

        ConfigurarMinioMockSemCorrelacao(linkDownload);

        // Act
        var result = await _templateExcel.GerarExcelEF(relatorioDto);

        // Assert
        result.Should().Be(linkDownload);
        _mockerServicoArmazenamentoMinio.Verify(s => s.UploadRelatorioAsync(
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        ), Times.Once);
    }

    [Fact]
    public async Task GerarExcelEF_DeveGerarExcelComSucesso_ComDadosPorGenero()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterDtoComGeneros(codigoCorrelacao);
        var linkDownload = "http://minio/relatorio-consolidado-genero.xlsx";

        ConfigurarMinioMock(codigoCorrelacao, linkDownload);

        // Act
        var result = await _templateExcel.GerarExcelEF(relatorioDto);

        // Assert
        result.Should().Be(linkDownload);
        _mockerServicoArmazenamentoMinio.Verify(s => s.UploadRelatorioAsync(
            It.IsAny<byte[]>(),
            It.Is<string>(n => n.Contains(codigoCorrelacao.ToString())),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        ), Times.Once);
    }

    [Fact]
    public async Task GerarExcelEF_DeveGerarExcelComSucesso_ComDadosPorRaca()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterDtoComRacas(codigoCorrelacao);
        var linkDownload = "http://minio/relatorio-consolidado-raca.xlsx";

        ConfigurarMinioMock(codigoCorrelacao, linkDownload);

        // Act
        var result = await _templateExcel.GerarExcelEF(relatorioDto);

        // Assert
        result.Should().Be(linkDownload);
        _mockerServicoArmazenamentoMinio.Verify(s => s.UploadRelatorioAsync(
            It.IsAny<byte[]>(),
            It.Is<string>(n => n.Contains(codigoCorrelacao.ToString())),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        ), Times.Once);
    }

    [Fact]
    public async Task GerarExcelEF_DeveGerarExcelComSucesso_ComDadosPorGeneroComRacas()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterDtoComGenerosComRacas(codigoCorrelacao);
        var linkDownload = "http://minio/relatorio-consolidado-genero-raca.xlsx";

        ConfigurarMinioMock(codigoCorrelacao, linkDownload);

        // Act
        var result = await _templateExcel.GerarExcelEF(relatorioDto);

        // Assert
        result.Should().Be(linkDownload);
        _mockerServicoArmazenamentoMinio.Verify(s => s.UploadRelatorioAsync(
            It.IsAny<byte[]>(),
            It.Is<string>(n => n.Contains(codigoCorrelacao.ToString())),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        ), Times.Once);
    }

    [Fact]
    public async Task GerarExcelEF_DeveGerarExcel_ComMultiplasQuestoesMesmoAno()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterDtoComMultiplasQuestoesMesmoAno(codigoCorrelacao);
        var linkDownload = "http://minio/relatorio-consolidado-multiplas.xlsx";

        ConfigurarMinioMock(codigoCorrelacao, linkDownload);

        // Act
        var result = await _templateExcel.GerarExcelEF(relatorioDto);

        // Assert
        result.Should().Be(linkDownload);
    }

    [Fact]
    public async Task GerarExcelEF_DeveGerarExcel_ComMultiplasQuestoesDiferentesAnos()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterDtoComQuestoesDiferentesAnos(codigoCorrelacao);
        var linkDownload = "http://minio/relatorio-consolidado-anos.xlsx";

        ConfigurarMinioMock(codigoCorrelacao, linkDownload);

        // Act
        var result = await _templateExcel.GerarExcelEF(relatorioDto);

        // Assert
        result.Should().Be(linkDownload);
    }

    [Fact]
    public async Task GerarExcelEF_DeveGerarExcel_ComRespostaSemCores()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterDtoComBimestres(codigoCorrelacao);
        foreach (var questao in relatorioDto.Questoes)
            foreach (var resposta in questao.Respostas ?? [])
            {
                resposta.CorFundo = null;
                resposta.CorTexto = null;
            }

        var linkDownload = "http://minio/relatorio-consolidado-sem-cores.xlsx";
        ConfigurarMinioMock(codigoCorrelacao, linkDownload);

        // Act
        var result = await _templateExcel.GerarExcelEF(relatorioDto);

        // Assert
        result.Should().Be(linkDownload);
    }

    [Fact]
    public async Task GerarExcelEF_DeveGerarExcel_QuandoQuestaoNomeSemParenteses()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterDtoComBimestres(codigoCorrelacao);
        relatorioDto.Questoes.First().QuestaoNome = "Escrita sem ano";

        var linkDownload = "http://minio/relatorio.xlsx";
        ConfigurarMinioMock(codigoCorrelacao, linkDownload);

        // Act
        var result = await _templateExcel.GerarExcelEF(relatorioDto);

        // Assert
        result.Should().Be(linkDownload);
    }

    private void ConfigurarMinioMock(Guid codigoCorrelacao, string linkDownload)
    {
        _mockerServicoArmazenamentoMinio.Setup(s => s.UploadRelatorioAsync(
            It.IsAny<byte[]>(),
            It.Is<string>(n => n.Contains(codigoCorrelacao.ToString())),
            It.IsAny<string>()
        )).ReturnsAsync(linkDownload);

        _mockerServicoArmazenamentoMinio.Setup(s => s.GerarLinkDownloadAsync(
            It.Is<string>(n => n.Contains(codigoCorrelacao.ToString())),
            It.IsAny<int>()
        )).ReturnsAsync(linkDownload);
    }

    private void ConfigurarMinioMockSemCorrelacao(string linkDownload)
    {
        _mockerServicoArmazenamentoMinio.Setup(s => s.UploadRelatorioAsync(
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync(linkDownload);

        _mockerServicoArmazenamentoMinio.Setup(s => s.GerarLinkDownloadAsync(
            It.IsAny<string>(),
            It.IsAny<int>()
        )).ReturnsAsync(linkDownload);
    }

    private static RelatorioConsolidadoSondagemDto ObterDtoBase(Guid codigoCorrelacao) =>
        new()
        {
            CodigoCorrelacao = codigoCorrelacao,
            AnoLetivo = 2024,
            Modalidade = "Fundamental",
            Dre = "DRE - BT",
            UnidadeEducacional = "EMEF Teste",
            AnoTurma = "3° ANO",
            ComponenteCurricular = "Língua Portuguesa",
            Proficiencia = "Escrita",
            Bimestre = "1° bimestre",
            Agrupamento = "Genérico",
            Usuario = "Professor Teste",
            DataImpressao = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };

    private static RelatorioConsolidadoSondagemDto ObterDtoComBimestres(Guid codigoCorrelacao)
    {
        var dto = ObterDtoBase(codigoCorrelacao);
        dto.Questoes =
        [
            new RelatorioConsolidadoQuestaoDto
            {
                QuestaoId = 1,
                QuestaoNome = "Escrita (3° ANO)",
                Respostas =
                [
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Pré-silábico",
                        CorFundo = "#FF0000",
                        CorTexto = "#FFFFFF",
                        Total = 10,
                        Percentual = 50.0,
                        Ordem = 1,
                        Bimestres =
                        [
                            new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 6, Percentual = 30.0 },
                            new RelatorioConsolidadoBimestreDto { Bimestre = "2° bimestre", Quantidade = 4, Percentual = 20.0 }
                        ]
                    },
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Silábico",
                        CorFundo = "#00FF00",
                        CorTexto = "#000000",
                        Total = 10,
                        Percentual = 50.0,
                        Ordem = 2,
                        Bimestres =
                        [
                            new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 5, Percentual = 25.0 },
                            new RelatorioConsolidadoBimestreDto { Bimestre = "2° bimestre", Quantidade = 5, Percentual = 25.0 }
                        ]
                    }
                ],
                TotaisPorBimestre =
                [
                    new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 11, Percentual = 55.0 },
                    new RelatorioConsolidadoBimestreDto { Bimestre = "2° bimestre", Quantidade = 9, Percentual = 45.0 }
                ]
            }
        ];
        return dto;
    }

    private static RelatorioConsolidadoSondagemDto ObterDtoComGeneros(Guid codigoCorrelacao)
    {
        var dto = ObterDtoBase(codigoCorrelacao);
        dto.Questoes =
        [
            new RelatorioConsolidadoQuestaoDto
            {
                QuestaoId = 2,
                QuestaoNome = "Escrita (3° ANO)",
                Respostas =
                [
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Pré-silábico",
                        CorFundo = "#FF0000",
                        CorTexto = "#FFFFFF",
                        Total = 15,
                        Percentual = 50.0,
                        Ordem = 1,
                        Generos =
                        [
                            new RelatorioConsolidadoGeneroDto { Genero = "Feminino", Sigla = "F", Quantidade = 8, Percentual = 53.3 },
                            new RelatorioConsolidadoGeneroDto { Genero = "Masculino", Sigla = "M", Quantidade = 7, Percentual = 46.7 }
                        ]
                    },
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Silábico",
                        CorFundo = "#00FF00",
                        CorTexto = "#000000",
                        Total = 15,
                        Percentual = 50.0,
                        Ordem = 2,
                        Generos =
                        [
                            new RelatorioConsolidadoGeneroDto { Genero = "Feminino", Sigla = "F", Quantidade = 9, Percentual = 60.0 },
                            new RelatorioConsolidadoGeneroDto { Genero = "Masculino", Sigla = "M", Quantidade = 6, Percentual = 40.0 }
                        ]
                    }
                ],
                TotaisPorGenero =
                [
                    new RelatorioConsolidadoGeneroDto { Genero = "Feminino", Sigla = "F", Quantidade = 17, Percentual = 56.7 },
                    new RelatorioConsolidadoGeneroDto { Genero = "Masculino", Sigla = "M", Quantidade = 13, Percentual = 43.3 }
                ]
            }
        ];
        return dto;
    }

    private static RelatorioConsolidadoSondagemDto ObterDtoComRacas(Guid codigoCorrelacao)
    {
        var dto = ObterDtoBase(codigoCorrelacao);
        dto.Questoes =
        [
            new RelatorioConsolidadoQuestaoDto
            {
                QuestaoId = 3,
                QuestaoNome = "Escrita (3° ANO)",
                Respostas =
                [
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Pré-silábico",
                        CorFundo = "#FF0000",
                        CorTexto = "#FFFFFF",
                        Total = 20,
                        Percentual = 40.0,
                        Ordem = 1,
                        Racas =
                        [
                            new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 8, Percentual = 40.0 },
                            new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 7, Percentual = 35.0 },
                            new RelatorioConsolidadoRacaDto { Raca = "Preta", Quantidade = 5, Percentual = 25.0 }
                        ]
                    },
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Silábico",
                        CorFundo = "#00FF00",
                        CorTexto = "#000000",
                        Total = 30,
                        Percentual = 60.0,
                        Ordem = 2,
                        Racas =
                        [
                            new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 12, Percentual = 40.0 },
                            new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 10, Percentual = 33.3 },
                            new RelatorioConsolidadoRacaDto { Raca = "Preta", Quantidade = 8, Percentual = 26.7 }
                        ]
                    }
                ],
                TotaisPorRaca =
                [
                    new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 20, Percentual = 40.0 },
                    new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 17, Percentual = 34.0 },
                    new RelatorioConsolidadoRacaDto { Raca = "Preta", Quantidade = 13, Percentual = 26.0 }
                ]
            }
        ];
        return dto;
    }

    private static RelatorioConsolidadoSondagemDto ObterDtoComGenerosComRacas(Guid codigoCorrelacao)
    {
        var dto = ObterDtoBase(codigoCorrelacao);
        dto.Questoes =
        [
            new RelatorioConsolidadoQuestaoDto
            {
                QuestaoId = 4,
                QuestaoNome = "Escrita (3° ANO)",
                Respostas =
                [
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Pré-silábico",
                        CorFundo = "#FF0000",
                        CorTexto = "#FFFFFF",
                        Total = 20,
                        Percentual = 40.0,
                        Ordem = 1,
                        GenerosComRacas =
                        [
                            new RelatorioConsolidadoGeneroRacaDto
                            {
                                Genero = "Feminino",
                                TotalGenero = 10,
                                PercentualGenero = 50.0,
                                Racas =
                                [
                                    new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 6, Percentual = 60.0 },
                                    new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 4, Percentual = 40.0 }
                                ]
                            },
                            new RelatorioConsolidadoGeneroRacaDto
                            {
                                Genero = "Masculino",
                                TotalGenero = 10,
                                PercentualGenero = 50.0,
                                Racas =
                                [
                                    new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 5, Percentual = 50.0 },
                                    new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 5, Percentual = 50.0 }
                                ]
                            }
                        ]
                    },
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Silábico",
                        CorFundo = "#00FF00",
                        CorTexto = "#000000",
                        Total = 30,
                        Percentual = 60.0,
                        Ordem = 2,
                        GenerosComRacas =
                        [
                            new RelatorioConsolidadoGeneroRacaDto
                            {
                                Genero = "Feminino",
                                TotalGenero = 15,
                                PercentualGenero = 50.0,
                                Racas =
                                [
                                    new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 9, Percentual = 60.0 },
                                    new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 6, Percentual = 40.0 }
                                ]
                            },
                            new RelatorioConsolidadoGeneroRacaDto
                            {
                                Genero = "Masculino",
                                TotalGenero = 15,
                                PercentualGenero = 50.0,
                                Racas =
                                [
                                    new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 8, Percentual = 53.3 },
                                    new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 7, Percentual = 46.7 }
                                ]
                            }
                        ]
                    }
                ],
                TotaisPorGeneroComRacas =
                [
                    new RelatorioConsolidadoGeneroRacaDto
                    {
                        Genero = "Feminino",
                        TotalGenero = 25,
                        PercentualGenero = 50.0,
                        Racas =
                        [
                            new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 15, Percentual = 60.0 },
                            new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 10, Percentual = 40.0 }
                        ]
                    },
                    new RelatorioConsolidadoGeneroRacaDto
                    {
                        Genero = "Masculino",
                        TotalGenero = 25,
                        PercentualGenero = 50.0,
                        Racas =
                        [
                            new RelatorioConsolidadoRacaDto { Raca = "Branca", Quantidade = 13, Percentual = 52.0 },
                            new RelatorioConsolidadoRacaDto { Raca = "Parda", Quantidade = 12, Percentual = 48.0 }
                        ]
                    }
                ]
            }
        ];
        return dto;
    }

    private static RelatorioConsolidadoSondagemDto ObterDtoComMultiplasQuestoesMesmoAno(Guid codigoCorrelacao)
    {
        var dto = ObterDtoBase(codigoCorrelacao);
        dto.Questoes =
        [
            new RelatorioConsolidadoQuestaoDto
            {
                QuestaoId = 1,
                QuestaoNome = "Escrita (3° ANO)",
                Respostas =
                [
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Pré-silábico",
                        Total = 10,
                        Bimestres = [ new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 10, Percentual = 100.0 } ]
                    }
                ],
                TotaisPorBimestre = [ new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 10, Percentual = 100.0 } ]
            },
            new RelatorioConsolidadoQuestaoDto
            {
                QuestaoId = 2,
                QuestaoNome = "Leitura (3° ANO)",
                Respostas =
                [
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Leitor",
                        Total = 8,
                        Bimestres = [ new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 8, Percentual = 100.0 } ]
                    }
                ],
                TotaisPorBimestre = [ new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 8, Percentual = 100.0 } ]
            }
        ];
        return dto;
    }

    private static RelatorioConsolidadoSondagemDto ObterDtoComQuestoesDiferentesAnos(Guid codigoCorrelacao)
    {
        var dto = ObterDtoBase(codigoCorrelacao);
        dto.Questoes =
        [
            new RelatorioConsolidadoQuestaoDto
            {
                QuestaoId = 1,
                QuestaoNome = "Escrita (3° ANO)",
                Respostas =
                [
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Pré-silábico",
                        Total = 10,
                        Bimestres = [ new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 10, Percentual = 100.0 } ]
                    }
                ],
                TotaisPorBimestre = [ new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 10, Percentual = 100.0 } ]
            },
            new RelatorioConsolidadoQuestaoDto
            {
                QuestaoId = 3,
                QuestaoNome = "Escrita (4° ANO)",
                Respostas =
                [
                    new RelatorioConsolidadoRespostaDto
                    {
                        Resposta = "Silábico",
                        Total = 12,
                        Bimestres = [ new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 12, Percentual = 100.0 } ]
                    }
                ],
                TotaisPorBimestre = [ new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 12, Percentual = 100.0 } ]
            }
        ];
        return dto;
    }
}
