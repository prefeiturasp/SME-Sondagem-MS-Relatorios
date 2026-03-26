using FluentAssertions;
using Moq;
using SME.Sondagem.MS.Relatorios.Infra.Constantes;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Records;
using SME.Sondagem.MS.Relatorios.Infra.Services;
using System.Net;
using System.Text.Json;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Services;

public class ServicoSondagemApiClientTeste
{
    private static (Mock<IHttpClientFactory> factory, MockHttpMessageHandler handler) CriarFactory(
        HttpStatusCode status, string? json = null)
    {
        var handler = new MockHttpMessageHandler(status, json ?? string.Empty);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://fakesondagem/") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(ServicoSondagemConstantes.SERVICO)).Returns(httpClient);
        return (factory, handler);
    }

    private static FiltroRelatorioSondagemPorTurmaDto CriarFiltro() =>
        new() { TurmaId = 1, AnoLetivo = 2024, Modalidade = 5 };

    [Fact]
    public async Task ObterDadosQuestionarioAsync_DeveRetornarDtoVazio_QuandoJsonForInvalido()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.OK, "{}");
        var service = new ServicoSondagemApiClient(factory.Object);

        var resultado = await service.ObterDadosQuestionarioAsync(CriarFiltro());

        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterDadosQuestionarioAsync_DeveRetornarDados_QuandoRespostaForValida()
    {
        var retorno = new RetornoApiSondagemQuestionarioDto(
            "Proficiência",
            "1",
            "2",
            new List<Estudante>(),
            new List<Legenda>(),
            42);
        var json = JsonSerializer.Serialize(retorno);
        var (factory, _) = CriarFactory(HttpStatusCode.OK, json);
        var service = new ServicoSondagemApiClient(factory.Object);

        var resultado = await service.ObterDadosQuestionarioAsync(CriarFiltro());

        resultado.TituloTabelaRespostas.Should().Be("Proficiência");
        resultado.QuestionarioId.Should().Be(42);
        resultado.SemestreId.Should().Be("1");
    }

    [Fact]
    public async Task ObterParametrosSondagemPorQuestionarioId_DeveRetornarListaVazia_QuandoRespostaForVazia()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.OK, "[]");
        var service = new ServicoSondagemApiClient(factory.Object);

        var resultado = await service.ObterParametrosSondagemPorQuestionarioId(1);

        resultado.Should().NotBeNull();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterParametrosSondagemPorQuestionarioId_DeveRetornarParametros_QuandoRespostaForValida()
    {
        var parametros = new List<ParametroSondagemDto>
        {
            new() { Valor = "Param1" },
            new() { Valor = "Param2" }
        };
        var json = JsonSerializer.Serialize(parametros);
        var (factory, _) = CriarFactory(HttpStatusCode.OK, json);
        var service = new ServicoSondagemApiClient(factory.Object);

        var resultado = await service.ObterParametrosSondagemPorQuestionarioId(99);

        resultado.Should().HaveCount(2);
        resultado![0].Valor.Should().Be("Param1");
    }

    [Fact]
    public async Task ObterParametrosSondagemPorQuestionarioId_DevePropagaCancellationToken_QuandoSolicitado()
    {
        var json = "[]";
        var (factory, _) = CriarFactory(HttpStatusCode.OK, json);
        var service = new ServicoSondagemApiClient(factory.Object);

        var cts = new CancellationTokenSource();
        var resultado = await service.ObterParametrosSondagemPorQuestionarioId(1, cts.Token);

        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterProficienciaPorIdAsync_DeveRetornarDtoVazio_QuandoStatusNaoForSucesso()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.InternalServerError, string.Empty);
        var service = new ServicoSondagemApiClient(factory.Object);

        var resultado = await service.ObterProficienciaPorIdAsync(10);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().BeEmpty();
        resultado.ComponenteCurricularId.Should().Be(0);
    }

    [Fact]
    public async Task ObterProficienciaPorIdAsync_DeveRetornarDtoVazio_QuandoStatusForNoContent()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.NoContent, string.Empty);
        var service = new ServicoSondagemApiClient(factory.Object);

        var resultado = await service.ObterProficienciaPorIdAsync(10);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().BeEmpty();
        resultado.ComponenteCurricularId.Should().Be(0);
    }

    [Fact]
    public async Task ObterProficienciaPorIdAsync_DeveRetornarDados_QuandoRespostaForValida()
    {
        var dto = new ProficienciaDto
        {
            Nome = "Avançado",
            ComponenteCurricularId = 3
        };

        var json = JsonSerializer.Serialize(dto);
        var (factory, _) = CriarFactory(HttpStatusCode.OK, json);
        var service = new ServicoSondagemApiClient(factory.Object);

        var resultado = await service.ObterProficienciaPorIdAsync(5);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Avançado");
        resultado.ComponenteCurricularId.Should().Be(3);
    }

    [Fact]
    public async Task ObterProficienciaPorIdAsync_DeveRetornarDtoVazio_QuandoJsonForInvalido()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.OK, "{}");
        var service = new ServicoSondagemApiClient(factory.Object);

        var resultado = await service.ObterProficienciaPorIdAsync(5);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().BeEmpty();
        resultado.ComponenteCurricularId.Should().Be(0);
    }

    [Fact]
    public async Task ObterProficienciaPorIdAsync_DevePropagarCancellationToken_QuandoSolicitado()
    {
        var dto = new ProficienciaDto { Nome = "Teste", ComponenteCurricularId = 1 };
        var json = JsonSerializer.Serialize(dto);

        var (factory, _) = CriarFactory(HttpStatusCode.OK, json);
        var service = new ServicoSondagemApiClient(factory.Object);

        var cts = new CancellationTokenSource();

        var resultado = await service.ObterProficienciaPorIdAsync(1, cts.Token);

        resultado.Should().NotBeNull();
    }
}
