using Moq;
using Xunit;
using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Excel.Templates;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Dtos.Questionario;

namespace SME.Sondagem.MS.Relatorios.Excel.Teste.Templates;

public class RelatorioSondagemQuestionarioPorTurmaTemplateExcelTeste
{
    private readonly Mock<IServicoArmazenamentoMinio> _mockerServicoArmazenamentoMinio;
    private readonly RelatorioSondagemQuestionarioPorTurmaTemplateExcel _templateExcel;

    public RelatorioSondagemQuestionarioPorTurmaTemplateExcelTeste()
    {
        _mockerServicoArmazenamentoMinio = new Mock<IServicoArmazenamentoMinio>();
        _templateExcel = new RelatorioSondagemQuestionarioPorTurmaTemplateExcel(_mockerServicoArmazenamentoMinio.Object);
    }

    [Fact]
    public async Task GerarExcelEF_DeveGerarExcelComSucesso()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterRelatorioSondagemPorTurmaDto(codigoCorrelacao);
        var linkDownload = "http://minio/relatorio.xlsx";

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
    public async Task GerarExcelEF_DeveTratarEstudantesSemRespostas()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var relatorioDto = ObterRelatorioSondagemPorTurmaDto(codigoCorrelacao);
        relatorioDto.Estudantes.First().Coluna = new List<ColunaQuestionarioDto>(); // Sem respostas

        var linkDownload = "http://minio/relatorio.xlsx";

        _mockerServicoArmazenamentoMinio.Setup(s => s.UploadRelatorioAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(linkDownload);
        _mockerServicoArmazenamentoMinio.Setup(s => s.GerarLinkDownloadAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(linkDownload);

        // Act
        var result = await _templateExcel.GerarExcelEF(relatorioDto);

        // Assert
        result.Should().Be(linkDownload);
    }

    private static RelatorioSondagemPorTurmaDto ObterRelatorioSondagemPorTurmaDto(Guid codigoCorrelacao)
    {
        return new RelatorioSondagemPorTurmaDto
        {
            AnoLetivo = 2024,
            Bimestre = 1,
            CodigoCorrelacao = codigoCorrelacao,
            DataImpressao = DateTime.Now,
            Dre = "DRE - BT",
            SiglaDre = "BT",
            Modalidade = Modalidade.Fundamental,
            Proficiencia = "Leitura",
            Turma = "3A",
            UnidadeEducacional = "EMEF Teste",
            Usuario = "Professor Teste",
            ExibeColunaLinguaPortuguesaSegundaLingua = true,
            Estudantes = new List<EstudanteDto>
            {
                new EstudanteDto
                {
                    NomeRelatorio = "Estudante 1",
                    NumeroAlunoChamada = "1",
                    Raca = "Parda",
                    Genero = "Masculino",
                    LinguaPortuguesaSegundaLingua = true,
                    Coluna = new List<ColunaQuestionarioDto>
                    {
                        new ColunaQuestionarioDto
                        {
                            DescricaoColuna = "Questão 1",
                            OpcaoResposta = new List<OpcaoRespostaDto>
                            {
                                new OpcaoRespostaDto { Id = 1, DescricaoOpcaoResposta = "Alfabético", CorFundo = "#00FF00" }
                            },
                            Resposta = new RespostaDto { OpcaoRespostaId = 1 }
                        }
                    }
                }
            }
        };
    }
}
