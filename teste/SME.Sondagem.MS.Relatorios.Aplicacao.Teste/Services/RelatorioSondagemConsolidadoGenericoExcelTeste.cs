using FluentAssertions;
using Moq;
using SME.Sondagem.MS.Relatorios.Aplicacao.Services;
using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Teste.Services;

public class RelatorioSondagemConsolidadoGenericoExcelTeste
{
    private readonly Mock<IRelatorioSondagemConsolidadoGenericoTemplateExcel> _templateMock = new();
    private readonly RelatorioSondagemConsolidadoGenericoExcel _service;

    public RelatorioSondagemConsolidadoGenericoExcelTeste()
    {
        _service = new RelatorioSondagemConsolidadoGenericoExcel(_templateMock.Object);
    }

    [Fact]
    public async Task GerarRelatorioExcelAsync_DeveRetornarLink_QuandoSucesso()
    {
        var dto = new RelatorioConsolidadoSondagemDto();
        var linkEsperado = "link_para_planilha_consolidado_generico";

        _templateMock
            .Setup(x => x.GerarExcelEF(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .ReturnsAsync(linkEsperado);

        var result = await _service.GerarRelatorioExcelAsync(dto);

        result.Should().Be(linkEsperado);
        _templateMock.Verify(x => x.GerarExcelEF(dto), Times.Once);
    }

    [Fact]
    public async Task GerarRelatorioExcelAsync_DeveDelegarAoTemplate_ComMesmoDtoRecebido()
    {
        var dto = new RelatorioConsolidadoSondagemDto { CodigoCorrelacao = Guid.NewGuid() };
        RelatorioConsolidadoSondagemDto? capturado = null;

        _templateMock
            .Setup(x => x.GerarExcelEF(It.IsAny<RelatorioConsolidadoSondagemDto>()))
            .Callback<RelatorioConsolidadoSondagemDto>(d => capturado = d)
            .ReturnsAsync("url");

        await _service.GerarRelatorioExcelAsync(dto);

        capturado.Should().BeSameAs(dto);
    }
}
