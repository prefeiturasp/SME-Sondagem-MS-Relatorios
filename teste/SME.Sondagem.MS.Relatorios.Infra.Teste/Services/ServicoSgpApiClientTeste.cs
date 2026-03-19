using FluentAssertions;
using Moq;
using SME.Sondagem.MS.Relatorios.Infra.Constantes;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Services;
using System.Net;
using System.Text.Json;
using Xunit;

namespace SME.Sondagem.MS.Relatorios.Infra.Teste.Services;

public class ServicoSgpApiClientTeste
{
    private static (Mock<IHttpClientFactory> factory, MockHttpMessageHandler handler) CriarFactory(
        HttpStatusCode status, string? json = null)
    {
        var handler = new MockHttpMessageHandler(status, json ?? string.Empty);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://fakesgp/") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(ServicoSgpConstantes.SERVICO)).Returns(httpClient);
        return (factory, handler);
    }

    [Fact]
    public async Task FinalizarSolicitacaoRelatorioAsync_DeveFinalizarComSucesso_QuandoRespostaForOkEBodyVazio()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.OK, "");
        var service = new ServicoSgpApiClient(factory.Object);
        var dto = new FinalizarSolicitacaoRelatorioDto(1, "http://relatorio.url", Guid.NewGuid());

        var acao = () => service.FinalizarSolicitacaoRelatorioAsync(dto);

        await acao.Should().NotThrowAsync();
    }

    [Fact]
    public async Task FinalizarSolicitacaoRelatorioAsync_DeveLancarExcecao_QuandoRespostaForErro()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.InternalServerError);
        var service = new ServicoSgpApiClient(factory.Object);
        var dto = new FinalizarSolicitacaoRelatorioDto(1, "http://relatorio.url", Guid.NewGuid());

        var acao = () => service.FinalizarSolicitacaoRelatorioAsync(dto);

        await acao.Should().ThrowAsync<Exception>()
            .WithMessage("*Erro ao consultar API de sondagem*");
    }

    [Fact]
    public async Task FinalizarSolicitacaoRelatorioAsync_DeveLancarExcecao_QuandoRespostaForNoContent()
    {
        var (factory, _) = CriarFactory(HttpStatusCode.NoContent);
        var service = new ServicoSgpApiClient(factory.Object);
        var dto = new FinalizarSolicitacaoRelatorioDto(1, "http://relatorio.url", Guid.NewGuid());

        var acao = () => service.FinalizarSolicitacaoRelatorioAsync(dto);

        await acao.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task FinalizarSolicitacaoRelatorioAsync_DeveLancarExcecao_QuandoRespostaOkContemBody()
    {
        // A implementação lança exceção quando o body não está vazio (comportamento atual)
        var (factory, _) = CriarFactory(HttpStatusCode.OK, "{\"algumCampo\":\"valor\"}");
        var service = new ServicoSgpApiClient(factory.Object);
        var dto = new FinalizarSolicitacaoRelatorioDto(1, "http://relatorio.url", Guid.NewGuid());

        var acao = () => service.FinalizarSolicitacaoRelatorioAsync(dto);

        await acao.Should().ThrowAsync<Exception>()
            .WithMessage("*Erro ao finalizar solicitação*");
    }
}
