using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Fila;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Records;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Aplicacao.Teste.UseCases;

public class RelatorioSondagemQuestionarioPorTurmaUseCaseTeste
{
    private readonly Mock<IServicoSondagemApiClient> _servicoSondagemApiClientMock;
    private readonly Mock<IRelatorioSondagemQuestionarioPorTurmaPdf> _relatorioPdfMock;
    private readonly Mock<IRelatorioSondagemQuestionarioPorTurmaExcel> _relatorioExcelMock;
    private readonly Mock<IServicoSgpApiClient> _servicoSgpApiClientMock;
    private readonly Mock<IServicoEolApiClient> _servicoEolApiClientMock;
    private readonly Mock<IServicoMensageria> _servicoMensageriaMock;
    private readonly Mock<ILogger<RelatorioSondagemQuestionarioPorTurmaUseCase>> _loggerMock;
    
    private readonly RelatorioSondagemQuestionarioPorTurmaUseCase _useCase;

    public RelatorioSondagemQuestionarioPorTurmaUseCaseTeste()
    {
        _servicoSondagemApiClientMock = new Mock<IServicoSondagemApiClient>();
        _relatorioPdfMock = new Mock<IRelatorioSondagemQuestionarioPorTurmaPdf>();
        _relatorioExcelMock = new Mock<IRelatorioSondagemQuestionarioPorTurmaExcel>();
        _servicoSgpApiClientMock = new Mock<IServicoSgpApiClient>();
        _servicoEolApiClientMock = new Mock<IServicoEolApiClient>();
        _servicoMensageriaMock = new Mock<IServicoMensageria>();
        _loggerMock = new Mock<ILogger<RelatorioSondagemQuestionarioPorTurmaUseCase>>();

        _useCase = new RelatorioSondagemQuestionarioPorTurmaUseCase(
            _servicoSondagemApiClientMock.Object,
            _loggerMock.Object,
            _servicoSgpApiClientMock.Object,
            _relatorioPdfMock.Object,
            _relatorioExcelMock.Object,
            _servicoMensageriaMock.Object,
            _servicoEolApiClientMock.Object);
    }

    [Fact]
    public async Task Executar_DeveRetornarFalso_QuandoFiltrosRelatorioForNulo()
    {
        // Arrange
        var mensagemRabbit = new MensagemRabbit(null, Guid.NewGuid());

        // Act
        var result = await _useCase.Executar(mensagemRabbit);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Executar_DeveRetornarFalso_QuandoOcorrerExcecao()
    {
        // Arrange
        var mensagemSondagem = new MensagemSondagemPorTurmaDto
        {
            FiltrosUsados = new FiltroRelatorioSondagemPorTurmaDto { ExtensaoRelatorio = (int)ExtensaoRelatorio.Pdf }
        };
        var mensagemJson = JsonSerializer.Serialize(mensagemSondagem, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var mensagemRabbit = new MensagemRabbit(mensagemJson, Guid.NewGuid());

        _servicoSondagemApiClientMock.Setup(x => x.ObterDadosQuestionarioAsync(It.IsAny<FiltroRelatorioSondagemPorTurmaDto>()))
            .ThrowsAsync(new Exception("Erro de teste"));

        // Act
        var result = await _useCase.Executar(mensagemRabbit);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Executar_DeveRetornarVerdadeiro_QuandoExtensaoForPdf()
    {
        // Arrange
        var mensagemSondagem = SetupValidMensagemSondagem((int)ExtensaoRelatorio.Pdf);
        var mensagemRabbit = SetupMensagemRabbit(mensagemSondagem);

        SetupMocksHappyPath();

        // Act
        var result = await _useCase.Executar(mensagemRabbit);

        // Assert
        result.Should().BeTrue();
        _relatorioPdfMock.Verify(x => x.GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(It.IsAny<RelatorioSondagemPorTurmaDto>()), Times.Once);
        _servicoSgpApiClientMock.Verify(x => x.FinalizarSolicitacaoRelatorioAsync(It.IsAny<FinalizarSolicitacaoRelatorioDto>()), Times.Once);
        _servicoMensageriaMock.Verify(x => x.Publicar(It.IsAny<MensagemRabbit>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Executar_DeveRetornarVerdadeiro_QuandoExtensaoForXlsx()
    {
        // Arrange
        var mensagemSondagem = SetupValidMensagemSondagem((int)ExtensaoRelatorio.Xlsx);
        var mensagemRabbit = SetupMensagemRabbit(mensagemSondagem);

        SetupMocksHappyPath();

        // Act
        var result = await _useCase.Executar(mensagemRabbit);

        // Assert
        result.Should().BeTrue();
        _relatorioExcelMock.Verify(x => x.GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync(It.IsAny<RelatorioSondagemPorTurmaDto>()), Times.Once);
        _servicoSgpApiClientMock.Verify(x => x.FinalizarSolicitacaoRelatorioAsync(It.IsAny<FinalizarSolicitacaoRelatorioDto>()), Times.Once);
        _servicoMensageriaMock.Verify(x => x.Publicar(It.IsAny<MensagemRabbit>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    private void SetupMocksHappyPath()
    {
        var retornoApi = new RetornoApiSondagemQuestionarioDto(
            "Titulo Tabela",
            "1",
            "1",
            new List<Estudante>(),
            new List<Legenda>(),
            1);

        _servicoSondagemApiClientMock.Setup(x => x.ObterDadosQuestionarioAsync(It.IsAny<FiltroRelatorioSondagemPorTurmaDto>()))
            .ReturnsAsync(retornoApi);

        _servicoSondagemApiClientMock.Setup(x => x.ObterParametrosSondagemPorQuestionarioId(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParametroSondagemDto>());

        _servicoEolApiClientMock.Setup(x => x.ObterDadosDreAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<EscolaDto> { new EscolaDto() });

        _servicoEolApiClientMock.Setup(x => x.ObterDadosTurmaAsync(It.IsAny<int>()))
            .ReturnsAsync(new TurmaDto());

        _servicoEolApiClientMock.Setup(x => x.ObterDadosUsuarioAsync(It.IsAny<string>()))
            .ReturnsAsync(new DadosUsuarioDto());

        _relatorioPdfMock.Setup(x => x.GerarRelatorioSondagemQuestionarioPorTurmaPdfAsync(It.IsAny<RelatorioSondagemPorTurmaDto>()))
            .ReturnsAsync("link_pdf");

        _relatorioExcelMock.Setup(x => x.GerarRelatorioSondagemQuestionarioPorTurmaExcelAsync(It.IsAny<RelatorioSondagemPorTurmaDto>()))
            .ReturnsAsync("link_excel");

        _servicoSgpApiClientMock.Setup(x => x.FinalizarSolicitacaoRelatorioAsync(It.IsAny<FinalizarSolicitacaoRelatorioDto>()))
            .Returns(Task.CompletedTask);

        _servicoMensageriaMock.Setup(x => x.Publicar(It.IsAny<MensagemRabbit>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
    }

    private MensagemSondagemPorTurmaDto SetupValidMensagemSondagem(int extensaoRelatorio)
    {
        return new MensagemSondagemPorTurmaDto
        {
            FiltrosUsados = new FiltroRelatorioSondagemPorTurmaDto 
            { 
                ExtensaoRelatorio = extensaoRelatorio,
                UeCodigo = "123",
                TurmaId = 1,
                Modalidade = 1
            },
            UsuarioQueSolicitou = "1234567"
        };
    }

    private MensagemRabbit SetupMensagemRabbit(MensagemSondagemPorTurmaDto mensagemSondagem)
    {
        var mensagemJson = JsonSerializer.Serialize(mensagemSondagem, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return new MensagemRabbit(mensagemJson, Guid.NewGuid());
    }
}
