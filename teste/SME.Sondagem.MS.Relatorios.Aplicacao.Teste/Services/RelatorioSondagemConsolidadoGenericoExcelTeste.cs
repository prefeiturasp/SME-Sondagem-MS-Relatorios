using FluentAssertions;
using Moq;
using SME.Sondagem.MS.Relatorios.Aplicacao.Services;
using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Teste.Services;

public class RelatorioSondagemConsolidadoGenericoExcelTeste
{
    private readonly Mock<IRelatorioSondagemConsolidadoGenericoTemplateExcel> _relatorioTemplateExcelMock;
    private readonly RelatorioSondagemConsolidadoGenericoExcel _service;

    public RelatorioSondagemConsolidadoGenericoExcelTeste()
    {
        _relatorioTemplateExcelMock = new Mock<IRelatorioSondagemConsolidadoGenericoTemplateExcel>();
        _service = new RelatorioSondagemConsolidadoGenericoExcel(_relatorioTemplateExcelMock.Object);
    }

    [Fact]
    public async Task GerarRelatorioExcelAsync_DeveRetornarLink_QuandoSucesso()
    {
        // Arrange
        var dto = new RelatorioConsolidadoSondagemDto();
        var linkEsperado = "link_para_planilha_consolidado_generico";

        _relatorioTemplateExcelMock
            .Setup(x => x.GerarExcelEF(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .ReturnsAsync(linkEsperado);

        // Act
        var result = await _service.GerarRelatorioExcelAsync(dto);

        // Assert
        result.Should().Be(linkEsperado);
        _relatorioTemplateExcelMock.Verify(x => x.GerarExcelEF(It.IsAny<RelatorioConsolidadoSondagemDto>()), Times.Once);
    }
}
