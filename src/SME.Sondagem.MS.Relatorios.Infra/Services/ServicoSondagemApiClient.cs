using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using System.Net;
using System.Text.Json;

namespace SME.Sondagem.MS.Relatorios.Infra.Services;

public class ServicoSondagemApiClient : IServicoSondagemApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ServicoSondagemApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<RetornoApiSondagemQuestionarioDto> ObterDadosQuestionarioAsync(FiltroRelatorioSondagemQuestionarioPorTurmaDto filtroRelatorio)
    {
        var httpClient = _httpClientFactory.CreateClient(ServicoSondagemConstantes.SERVICO);

        string urlFinal = filtroRelatorio.ObjetoParaQueryStringExtensions(ServicoSondagemConstantes.URL_SOLICITACAO_RELATORIO);
        var resposta = await httpClient.GetAsync(urlFinal);

        if (!resposta.IsSuccessStatusCode || resposta.StatusCode == HttpStatusCode.NoContent)
            throw new Exception($"Erro ao consultar API de sondagem. Status: {resposta.StatusCode}");

        var json = await resposta.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(json))
            return JsonSerializer.Deserialize<RetornoApiSondagemQuestionarioDto>(json);

        return null;
    }
}
