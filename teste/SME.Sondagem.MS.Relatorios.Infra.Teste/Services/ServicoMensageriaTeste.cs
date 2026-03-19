using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;
using SME.Sondagem.MS.Relatorios.Infra.Fila;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Services;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Services;

public class ServicoMensageriaTeste
{
    private readonly Mock<IServicoTelemetria> _telemetriaMock;
    private readonly Mock<ILogger<ServicoMensageria>> _loggerMock;
    private readonly RabbitOptions _rabbitOptions;

    public ServicoMensageriaTeste()
    {
        _telemetriaMock = new Mock<IServicoTelemetria>();
        _loggerMock = new Mock<ILogger<ServicoMensageria>>();
        _rabbitOptions = new RabbitOptions
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/"
        };

        _telemetriaMock
            .Setup(t => t.RegistrarAsync(It.IsAny<Func<Task>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<Func<Task>, string, string, string>((acao, _, _, _) => acao());
    }

    private ServicoMensageria CriarServico() =>
        new(_rabbitOptions, _telemetriaMock.Object, _loggerMock.Object);

    [Fact]
    public void Construtor_DeveLancarArgumentNullException_QuandoRabbitOptionsForNulo()
    {
        var acao = () => new ServicoMensageria(null!, _telemetriaMock.Object, _loggerMock.Object);

        acao.Should().Throw<ArgumentNullException>()
            .WithParameterName("rabbitOptions");
    }

    [Fact]
    public void Construtor_DeveLancarArgumentNullException_QuandoServicoTelemetriaForNulo()
    {
        var acao = () => new ServicoMensageria(_rabbitOptions, null!, _loggerMock.Object);

        acao.Should().Throw<ArgumentNullException>()
            .WithParameterName("servicoTelemetria");
    }

    [Fact]
    public void Construtor_DeveLancarArgumentNullException_QuandoLoggerForNulo()
    {
        var acao = () => new ServicoMensageria(_rabbitOptions, _telemetriaMock.Object, null!);

        acao.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task Publicar_DeveRetornarVerdadeiro_EInvocarTelemetria()
    {
        var servico = CriarServico();
        var mensagem = new MensagemRabbit("{}", Guid.NewGuid());

        // O método delega a publicação real para o RabbitMQ via telemetria;
        // a ação real lançará exceção de conexão no ambiente de test, mas é tratada internamente.
        var resultado = await servico.Publicar(mensagem, "rota-teste", "exchange-teste", "acao-teste");

        resultado.Should().BeTrue();
        _telemetriaMock.Verify(
            t => t.RegistrarAsync(It.IsAny<Func<Task>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }
}
