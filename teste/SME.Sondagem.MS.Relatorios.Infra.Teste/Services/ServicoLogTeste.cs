using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Services;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Services;

public class ServicoLogTeste
{
    private readonly Mock<IServicoTelemetria> _telemetriaMock;
    private readonly Mock<ILogger<ServicoLog>> _loggerMock;
    private readonly RabbitLogOptions _rabbitLogOptions;

    public ServicoLogTeste()
    {
        _telemetriaMock = new Mock<IServicoTelemetria>();
        _loggerMock = new Mock<ILogger<ServicoLog>>();
        _rabbitLogOptions = new RabbitLogOptions
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        };

        _telemetriaMock
            .Setup(t => t.Registrar(It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<Action, string, string, string>((acao, _, _, _) => acao());
    }

    private ServicoLog CriarServico() =>
        new(_telemetriaMock.Object, _rabbitLogOptions, _loggerMock.Object);

    [Fact]
    public void Construtor_DeveLancarArgumentNullException_QuandoServicoTelemetriaForNulo()
    {
        var acao = () => new ServicoLog(null!, _rabbitLogOptions, _loggerMock.Object);

        acao.Should().Throw<ArgumentNullException>()
            .WithParameterName("servicoTelemetria");
    }

    [Fact]
    public void Construtor_DeveLancarArgumentNullException_QuandoRabbitLogOptionsForNulo()
    {
        var acao = () => new ServicoLog(_telemetriaMock.Object, null!, _loggerMock.Object);

        acao.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuracaoRabbitOptions");
    }

    [Fact]
    public void Construtor_DeveLancarArgumentNullException_QuandoLoggerForNulo()
    {
        var acao = () => new ServicoLog(_telemetriaMock.Object, _rabbitLogOptions, null!);

        acao.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Registrar_ComException_DeveInvocarTelemetria()
    {
        var servico = CriarServico();
        var ex = new InvalidOperationException("Erro de teste");

        servico.Registrar(ex);

        _telemetriaMock.Verify(
            t => t.Registrar(It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void Registrar_ComMensagemEException_DeveInvocarTelemetria()
    {
        var servico = CriarServico();
        var ex = new Exception("falha");

        servico.Registrar("Contexto do erro", ex);

        _telemetriaMock.Verify(
            t => t.Registrar(It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void Registrar_ComNivelEDetalhes_DeveInvocarTelemetria()
    {
        var servico = CriarServico();

        servico.Registrar(
            SME.Sondagem.MS.Relatorios.Dominio.Enums.LogNivel.Informacao,
            "Erro detectado",
            "Observação do erro",
            "stack trace simulado");

        _telemetriaMock.Verify(
            t => t.Registrar(It.IsAny<Action>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }
}
