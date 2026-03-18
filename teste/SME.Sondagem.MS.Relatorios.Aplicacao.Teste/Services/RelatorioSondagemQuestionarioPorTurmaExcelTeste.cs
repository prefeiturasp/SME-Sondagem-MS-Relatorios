using FluentAssertions;
using Moq;
using SME.Sondagem.MS.Relatorios.Aplicacao.Services;
using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Teste.Services;

public class RelatorioSondagemQuestionarioPorTurmaExcelTeste
{
    private readonly Mock<IRelatorioSondagemQuestionarioPorTurmaTemplateExcel> _relatorioTemplateExcelMock;
    private readonly RelatorioSondagemQuestionarioPorTurmaExcel _service;

    public RelatorioSondagemQuestionarioPorTurmaExcelTeste()
    {
        _relatorioTemplateExcelMock = new Mock<IRelatorioSondagemQuestionarioPorTurmaTemplateExcel>();
        _service = new RelatorioSondagemQuestionarioPorTurmaExcel(_relatorioTemplateExcelMock.Object);
    }

    [Fact]
    public async Task GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync_DeveRetornarString_QuandoSucesso()
    {
        // Arrange
        var dto = new RelatorioSondagemPorTurmaDto();
        var linkEsperado = "link_para_planilha_excel";

        _relatorioTemplateExcelMock
            .Setup(x => x.GerarExcelEF(It.IsAny<RelatorioSondagemPorTurmaDto>()))
            .ReturnsAsync(linkEsperado);

        // Act
        var result = await _service.GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync(dto);

        // Assert
        result.Should().Be(linkEsperado);
        _relatorioTemplateExcelMock.Verify(x => x.GerarExcelEF(It.IsAny<RelatorioSondagemPorTurmaDto>()), Times.Once);
    }
}
