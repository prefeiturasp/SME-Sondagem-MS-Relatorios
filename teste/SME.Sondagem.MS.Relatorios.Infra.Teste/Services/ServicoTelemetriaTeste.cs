using FluentAssertions;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;
using SME.Sondagem.MS.Relatorios.Infra.Services;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Services;

public class ServicoTelemetriaTeste
{
    private static ServicoTelemetria CriarServico(bool apm = false) =>
        new(new TelemetriaOptions { Apm = apm });

    [Fact]
    public void Construtor_DeveLancarArgumentNullException_QuandoTelemetriaOptionsForNulo()
    {
        var acao = () => new ServicoTelemetria(null!);

        acao.Should().Throw<ArgumentNullException>()
            .WithParameterName("telemetriaOptions");
    }

    [Fact]
    public void IniciarTransacao_DeveRetornarTransacaoComNome_QuandoApmDesativado()
    {
        var service = CriarServico();

        var transacao = service.IniciarTransacao("rota-teste");

        transacao.Should().NotBeNull();
        transacao.Nome.Should().Be("rota-teste");
        transacao.Sucesso.Should().BeTrue();
        transacao.Temporizador.Should().NotBeNull();
        transacao.InicioOperacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void FinalizarTransacao_NaoDeveLancarExcecao_QuandoApmDesativado()
    {
        var service = CriarServico();
        var transacao = service.IniciarTransacao("rota-teste");

        var acao = () => service.FinalizarTransacao(transacao);

        acao.Should().NotThrow();
    }

    [Fact]
    public void RegistrarExcecao_NaoDeveLancarExcecao_QuandoApmDesativado()
    {
        var service = CriarServico();
        var transacao = service.IniciarTransacao("rota-teste");

        var acao = () => service.RegistrarExcecao(transacao, new InvalidOperationException("teste"));

        acao.Should().NotThrow();
    }

    [Fact]
    public void Registrar_DeveExecutarAcao_QuandoApmDesativado()
    {
        var service = CriarServico();
        var executou = false;

        service.Registrar(() => executou = true, "acao", "telemetria", "valor");

        executou.Should().BeTrue();
    }

    [Fact]
    public async Task RegistrarAsync_DeveExecutarAcao_QuandoApmDesativado()
    {
        var service = CriarServico();
        var executou = false;

        await service.RegistrarAsync(async () =>
        {
            await Task.Delay(1);
            executou = true;
        }, "acao", "telemetria", "valor");

        executou.Should().BeTrue();
    }

    [Fact]
    public async Task RegistrarComRetornoAsync_DeveRetornarResultado_QuandoApmDesativado()
    {
        var service = CriarServico();
        var valorEsperado = "resultado";

        var resultado = await service.RegistrarComRetornoAsync<string>(
            async () => await Task.FromResult<object>(valorEsperado),
            "acao", "telemetria", "valor");

        ((string)resultado).Should().Be(valorEsperado);
    }

    [Fact]
    public async Task RegistrarComRetornoAsync_ComParametros_DeveRetornarResultado_QuandoApmDesativado()
    {
        var service = CriarServico();

        var resultado = await service.RegistrarComRetornoAsync<int>(
            async () => await Task.FromResult<object>(42),
            "acao", "telemetria", "valor", "parametros");

        ((int)resultado).Should().Be(42);
    }

    [Fact]
    public void RegistrarComRetorno_DeveRetornarResultado_QuandoApmDesativado()
    {
        var service = CriarServico();

        var resultado = service.RegistrarComRetorno<string>(
            () => "valor-sincrono",
            "acao", "telemetria", "valor");

        ((string)resultado).Should().Be("valor-sincrono");
    }

    [Fact]
    public void ServicoTelemetriaTransacao_DeveSerInicializadaCorretamente()
    {
        var transacao = new ServicoTelemetria.ServicoTelemetriaTransacao("minha-rota");

        transacao.Nome.Should().Be("minha-rota");
        transacao.Sucesso.Should().BeTrue();
        transacao.TransacaoApm.Should().BeNull();
    }
}
