using SME.Sondagem.MS.Relatorios.Infra.Constantes;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SME.Sondagem.MS.Relatorios.Infra.Services;

public class ServicoSgpApiClient : IServicoSgpApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ServicoSgpApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task FinalizarSolicitacaoRelatorioAsync(FinalizarSolicitacaoRelatorioDto finalizarSolicitacaoRelatorioDto)
    {
        var httpClient = _httpClientFactory.CreateClient(ServicoSgpConstantes.SERVICO);

        string url = ServicoSgpConstantes.URL_FINALIZAR_SOLICITACAO_RELATORIO;
        
        var body = JsonSerializer.Serialize(finalizarSolicitacaoRelatorioDto);

        var resposta = await httpClient.PatchAsync(url, new StringContent(body.ToString(), Encoding.UTF8, "application/json"));

        if (!resposta.IsSuccessStatusCode || resposta.StatusCode == HttpStatusCode.NoContent)
            throw new Exception($"Erro ao consultar API de sondagem. Status: {resposta.StatusCode}");

        var json = await resposta.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(json))
            throw new Exception($"Erro ao finalizar solicitação de relatório API do SGP. Status: {resposta.StatusCode}");
    }
}
