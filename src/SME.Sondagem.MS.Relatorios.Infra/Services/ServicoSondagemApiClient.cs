using SME.Sondagem.MS.Relatorios.Infra.Constantes;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Records;
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

    public async Task<RetornoApiSondagemQuestionarioDto> ObterDadosQuestionarioAsync(FiltroRelatorioSondagemPorTurmaDto filtroRelatorio)
    {
        var httpClient = _httpClientFactory.CreateClient(ServicoSondagemConstantes.SERVICO);

        string urlFinal = filtroRelatorio.ObjetoParaQueryStringExtensions(ServicoSondagemConstantes.URL_SOLICITACAO_RELATORIO);
        var resposta = await httpClient.GetAsync(urlFinal);

        if (!resposta.IsSuccessStatusCode || resposta.StatusCode == HttpStatusCode.NoContent)
           return new RetornoApiSondagemQuestionarioDto(string.Empty, string.Empty, string.Empty, new(), new(), 0);

        var jsonString = await resposta.Content.ReadAsStringAsync();
        var options = JsonSerializerExtensions.ObterConfigSerializer();

        return JsonSerializer.Deserialize<RetornoApiSondagemQuestionarioDto>(jsonString, options)
                 ?? new RetornoApiSondagemQuestionarioDto(string.Empty, string.Empty, string.Empty, new(), new(), 0);
    }

    public async Task<List<ParametroSondagemDto>?> ObterParametrosSondagemPorQuestionarioId(long questionoarioId, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(ServicoSondagemConstantes.SERVICO);

        string urlFinal = string.Format(ServicoSondagemConstantes.URL_PARAMETROS_SONDAGEM, questionoarioId);
        var resposta = await httpClient.GetAsync(urlFinal, cancellationToken);

        if (!resposta.IsSuccessStatusCode || resposta.StatusCode == HttpStatusCode.NoContent)
           return [];

        var jsonString = await resposta.Content.ReadAsStringAsync(cancellationToken);
        var options = JsonSerializerExtensions.ObterConfigSerializer();

        return JsonSerializer.Deserialize<List<ParametroSondagemDto>>(jsonString, options)
                 ?? [];
    }

    public async Task<ProficienciaDto?> ObterProficienciaPorIdAsync(int proficienciaId, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(ServicoSondagemConstantes.SERVICO);

        string urlFinal = string.Format(ServicoSondagemConstantes.URL_PROFICIENCIA, proficienciaId);
        var resposta = await httpClient.GetAsync(urlFinal, cancellationToken);

        if (!resposta.IsSuccessStatusCode || resposta.StatusCode == HttpStatusCode.NoContent)
            return new ProficienciaDto();

        var jsonString = await resposta.Content.ReadAsStringAsync(cancellationToken);
        var options = JsonSerializerExtensions.ObterConfigSerializer();

        return JsonSerializer.Deserialize<ProficienciaDto>(jsonString, options)
                 ?? new ProficienciaDto();
    }
}
