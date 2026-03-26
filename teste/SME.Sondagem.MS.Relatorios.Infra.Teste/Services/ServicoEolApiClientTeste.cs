using FluentAssertions;
using Moq;
using SME.Sondagem.MS.Relatorios.Infra.Constantes;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Services;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Services;

public class ServicoEolApiClientTeste
{
    private static (Mock<IHttpClientFactory> factory, MockHttpMessageHandler handler) CriarFactory(
        HttpStatusCode status, string? json = null)
    {
        var handler = new MockHttpMessageHandler(status, json ?? string.Empty);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://fakeeol/") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(ServicoEolConstantes.SERVICO)).Returns(httpClient);
        return (factory, handler);
    }

    [Fact]
    public async Task ObterDadosDreAsync_DeveRetornarListaVazia_QuandoCodigoUeForNulo()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.OK);
        var service = new ServicoEolApiClient(factory.Object);

        var resultado = await service.ObterDadosDreAsync(null!);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterDadosDreAsync_DeveRetornarListaVazia_QuandoRespostaForErro()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.InternalServerError);
        var service = new ServicoEolApiClient(factory.Object);

        var resultado = await service.ObterDadosDreAsync(new List<string> { "001" });

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterDadosDreAsync_DeveRetornarListaVazia_QuandoRespostaForNoContent()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.NoContent);
        var service = new ServicoEolApiClient(factory.Object);

        var resultado = await service.ObterDadosDreAsync(new List<string> { "001" });

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterDadosDreAsync_DeveRetornarEscolas_QuandoRespostaForValida()
    {
        var escolas = new List<EscolaDto>
        {
            new() { CodigoEscola = "001", NomeEscola = "Escola Teste", NomeDRE = "DRE Norte" }
        };
        var json = JsonSerializer.Serialize(escolas);
        var (factory, _) = CriarFactory(HttpStatusCode.OK, json);
        var service = new ServicoEolApiClient(factory.Object);

        var resultado = await service.ObterDadosDreAsync(new List<string> { "001" });

        resultado.Should().HaveCount(1);
        resultado[0].CodigoEscola.Should().Be("001");
        resultado[0].NomeEscola.Should().Be("Escola Teste");
    }

    [Fact]
    public async Task ObterDadosTurmaAsync_DeveRetornarTurmaVazia_QuandoRespostaForErro()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.NotFound);
        var service = new ServicoEolApiClient(factory.Object);

        var resultado = await service.ObterDadosTurmaAsync(123);

        resultado.Should().NotBeNull();
        resultado.Should().BeOfType<TurmaDto>();
    }

    [Fact]
    public async Task ObterDadosTurmaAsync_DeveRetornarTurma_QuandoRespostaForValida()
    {
        var turma = new TurmaDto { NomeTurma = "Turma A", AnoLetivo = 2024 };
        var json = JsonSerializer.Serialize(turma);
        var (factory, _) = CriarFactory(HttpStatusCode.OK, json);
        var service = new ServicoEolApiClient(factory.Object);

        var resultado = await service.ObterDadosTurmaAsync(123);

        resultado.NomeTurma.Should().Be("Turma A");
        resultado.AnoLetivo.Should().Be(2024);
    }

    [Fact]
    public async Task ObterDadosUsuarioAsync_DeveRetornarUsuarioVazio_QuandoRespostaForErro()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.Forbidden);
        var service = new ServicoEolApiClient(factory.Object);

        var resultado = await service.ObterDadosUsuarioAsync("123456");

        resultado.Should().NotBeNull();
        resultado.Should().BeOfType<DadosUsuarioDto>();
    }

    [Fact]
    public async Task ObterDadosUsuarioAsync_DeveRetornarUsuario_QuandoRespostaForValida()
    {
        var usuario = new DadosUsuarioDto { Nome = "Prof. Silva", CodigoRf = "654321" };
        var json = JsonSerializer.Serialize(usuario);
        var (factory, _) = CriarFactory(HttpStatusCode.OK, json);
        var service = new ServicoEolApiClient(factory.Object);

        var resultado = await service.ObterDadosUsuarioAsync("654321");

        resultado.Nome.Should().Be("Prof. Silva");
        resultado.CodigoRf.Should().Be("654321");
    }

    [Fact]
    public async Task ObterDadosDreAsync_DeveRetornarListaVazia_QuandoJsonForVazio()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.OK, "   ");
        var service = new ServicoEolApiClient(factory.Object);

        var resultado = await service.ObterDadosDreAsync(new List<string> { "001" });

        resultado.Should().BeEmpty();
    }
}
