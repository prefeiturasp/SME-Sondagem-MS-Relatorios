using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SME.Sondagem.MS.Relatorios.Aplicacao.Services;
using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Teste.Services;

public class RelatorioSondagemQuestionarioPorTurmaPdfTeste
{
    private readonly Mock<IReportConverter> _reportConverterMock;
    private readonly Mock<IServicoArmazenamentoMinio> _servicoArmazenamentoMinioMock;
    private readonly Mock<IRelatorioSondagemQuestionarioPorTurmaTemplatePdf> _relatorioTemplatePdfMock;
    private readonly RelatorioSondagemQuestionarioPorTurmaPdf _service;

    public RelatorioSondagemQuestionarioPorTurmaPdfTeste()
    {
        _reportConverterMock = new Mock<IReportConverter>();
        _servicoArmazenamentoMinioMock = new Mock<IServicoArmazenamentoMinio>();
        _relatorioTemplatePdfMock = new Mock<IRelatorioSondagemQuestionarioPorTurmaTemplatePdf>();

        _service = new RelatorioSondagemQuestionarioPorTurmaPdf(
            _reportConverterMock.Object,
            _servicoArmazenamentoMinioMock.Object,
            _relatorioTemplatePdfMock.Object);
    }

    [Fact]
    public async Task GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync_DeveGerarUploadERetornarLink_QuandoSucesso()
    {
        // Arrange
        var codigoCorrelacao = Guid.NewGuid();
        var dto = new RelatorioSondagemPorTurmaDto { CodigoCorrelacao = codigoCorrelacao };
        var htmlEsperado = "<html><body>Relatório</body></html>";
        var bytesEsperados = new byte[] { 1, 2, 3 };
        var linkEsperado = "link_para_relatorio_pdf";

        _relatorioTemplatePdfMock
            .Setup(x => x.GerarHtml(It.IsAny<RelatorioSondagemPorTurmaDto>()))
            .Returns(htmlEsperado);

        _reportConverterMock
            .Setup(x => x.GerarPdfEmMemoria(It.IsAny<string>()))
            .Returns(bytesEsperados);

        _servicoArmazenamentoMinioMock
            .Setup(x => x.UploadRelatorioAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("pdf_object_name");

        _servicoArmazenamentoMinioMock
            .Setup(x => x.GerarLinkDownloadAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(linkEsperado);

        // Act
        var result = await _service.GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(dto);

        // Assert
        result.Should().Be(linkEsperado);
        
        _relatorioTemplatePdfMock.Verify(x => x.GerarHtml(dto), Times.Once);
        _reportConverterMock.Verify(x => x.GerarPdfEmMemoria(htmlEsperado), Times.Once);
        _servicoArmazenamentoMinioMock.Verify(x => x.UploadRelatorioAsync(bytesEsperados, $"Relatorio/{codigoCorrelacao}.pdf", "application/pdf"), Times.Once);
        _servicoArmazenamentoMinioMock.Verify(x => x.GerarLinkDownloadAsync($"Relatorio/{codigoCorrelacao}.pdf", 1440), Times.Once);
    }
}
