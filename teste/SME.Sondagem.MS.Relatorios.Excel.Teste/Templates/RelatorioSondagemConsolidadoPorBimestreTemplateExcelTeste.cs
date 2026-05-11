using FluentAssertions;
using Moq;
using SME.Sondagem.MS.Relatorios.Excel.Templates;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Excel.Teste.Templates;

public class RelatorioSondagemConsolidadoPorBimestreTemplateExcelTeste
{
    private readonly Mock<IServicoArmazenamentoMinio> _mockerServicoArmazenamentoMinio;
    private readonly RelatorioSondagemConsolidadoPorBimestreTemplateExcel _templateExcel;

    public RelatorioSondagemConsolidadoPorBimestreTemplateExcelTeste()
    {
        _mockerServicoArmazenamentoMinio = new Mock<IServicoArmazenamentoMinio>();
        _templateExcel = new RelatorioSondagemConsolidadoPorBimestreTemplateExcel(_mockerServicoArmazenamentoMinio.Object);
    }

    [Fact]
    public async Task GerarExcelEF_DeveGerarExcelComSucesso()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterRelatorioConsolidadoSondagemDto(codigoCorrelacao);
        var linkDownload = "http://minio/relatorio-consolidado.xlsx";

        _mockerServicoArmazenamentoMinio.Setup(s => s.UploadRelatorioAsync(
            It.IsAny<byte[]>(),
            It.Is<string>(n => n.Contains(codigoCorrelacao.ToString())),
            It.IsAny<string>()
        )).ReturnsAsync(linkDownload);

        _mockerServicoArmazenamentoMinio.Setup(s => s.GerarLinkDownloadAsync(
            It.Is<string>(n => n.Contains(codigoCorrelacao.ToString())),
            It.IsAny<int>()
        )).ReturnsAsync(linkDownload);

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
        var relatorioDto = ObterRelatorioConsolidadoSondagemDto(codigoCorrelacao);
        relatorioDto.Questoes = [];
        var linkDownload = "http://minio/relatorio-consolidado.xlsx";

        _mockerServicoArmazenamentoMinio.Setup(s => s.UploadRelatorioAsync(
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync(linkDownload);

        _mockerServicoArmazenamentoMinio.Setup(s => s.GerarLinkDownloadAsync(
            It.IsAny<string>(),
            It.IsAny<int>()
        )).ReturnsAsync(linkDownload);

        // Act
        var result = await _templateExcel.GerarExcelEF(relatorioDto);

        // Assert
        result.Should().Be(linkDownload);
    }

    private static RelatorioConsolidadoSondagemDto ObterRelatorioConsolidadoSondagemDto(Guid codigoCorrelacao)
    {
        return new RelatorioConsolidadoSondagemDto
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
            Agrupamento = "Por bimestre",
            Usuario = "Professor Teste",
            DataImpressao = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            Questoes = new List<RelatorioConsolidadoQuestaoDto>
            {
                new RelatorioConsolidadoQuestaoDto
                {
                    QuestaoId = 1,
                    QuestaoNome = "Escrita (3° ANO)",
                    Respostas = new List<RelatorioConsolidadoRespostaDto>
                    {
                        new RelatorioConsolidadoRespostaDto
                        {
                            Resposta = "Pré-silábico",
                            CorFundo = "#FF0000",
                            CorTexto = "#FFFFFF",
                            Total = 10,
                            Percentual = 50.0,
                            Ordem = 1,
                            Bimestres = new List<RelatorioConsolidadoBimestreDto>
                            {
                                new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 10, Percentual = 50.0 }
                            }
                        },
                        new RelatorioConsolidadoRespostaDto
                        {
                            Resposta = "Silábico",
                            CorFundo = "#00FF00",
                            CorTexto = "#000000",
                            Total = 10,
                            Percentual = 50.0,
                            Ordem = 2,
                            Bimestres = new List<RelatorioConsolidadoBimestreDto>
                            {
                                new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 10, Percentual = 50.0 }
                            }
                        }
                    },
                    TotaisPorBimestre = new List<RelatorioConsolidadoBimestreDto>
                    {
                        new RelatorioConsolidadoBimestreDto { Bimestre = "1° bimestre", Quantidade = 20, Percentual = 100.0 }
                    }
                }
            }
        };
    }
}
