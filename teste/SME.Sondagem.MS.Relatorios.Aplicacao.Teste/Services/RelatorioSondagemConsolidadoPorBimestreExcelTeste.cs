using FluentAssertions;
using Moq;
using SME.Sondagem.MS.Relatorios.Aplicacao.Services;
using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Teste.Services;

public class RelatorioSondagemConsolidadoPorBimestreExcelTeste
{
    private readonly Mock<IRelatorioSondagemConsolidadoPorBimestreTemplateExcel> _relatorioTemplateExcelMock;
    private readonly RelatorioSondagemConsolidadoPorBimestreExcel _service;

    public RelatorioSondagemConsolidadoPorBimestreExcelTeste()
    {
        _relatorioTemplateExcelMock = new Mock<IRelatorioSondagemConsolidadoPorBimestreTemplateExcel>();
        _service = new RelatorioSondagemConsolidadoPorBimestreExcel(_relatorioTemplateExcelMock.Object);
    }

    [Fact]
    public async Task GerarRelatorioSondagemConsolidadoPorBimestreExcelAsync_DeveRetornarLink_QuandoSucesso()
    {
        // Arrange
        var dto = new RelatorioConsolidadoSondagemDto();
        var linkEsperado = "link_para_planilha_consolidado_bimestre";

        _relatorioTemplateExcelMock
            .Setup(x => x.GerarExcelEF(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .ReturnsAsync(linkEsperado);

        // Act
        var result = await _service.GerarRelatorioSondagemConsolidadoPorBimestreExcelAsync(dto);

        // Assert
        result.Should().Be(linkEsperado);
        _relatorioTemplateExcelMock.Verify(x => x.GerarExcelEF(It.IsAny<RelatorioConsolidadoSondagemDto>()), Times.Once);
    }
}
