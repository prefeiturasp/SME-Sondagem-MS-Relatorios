using Elastic.Apm.Api;
using SME.Sondagem.MS.Relatorios.Infra.Constantes;
using SME.Sondagem.MS.Relatorios.Infra.Dtos;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SME.Sondagem.MS.Relatorios.Infra.Services;

public class ServicoEolApiClient : IServicoEolApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ServicoEolApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<EscolaDto>> ObterDadosDreAsync(List<string> codigoUe)
    {
        var httpClient = _httpClientFactory.CreateClient(ServicoEolConstantes.SERVICO);

        string url = ServicoEolConstantes.URL_BUSCAR_ESCOLAS;

        if (codigoUe == null)
            return [];

        var body = JsonSerializer.Serialize(codigoUe);

        var resposta = await httpClient.PostAsync(url, new StringContent(body.ToString(), Encoding.UTF8, "application/json"));

        if (!resposta.IsSuccessStatusCode || resposta.StatusCode == HttpStatusCode.NoContent)
            return [];

        var json = await resposta.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
            return [];

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<EscolaDto>>(json, options);
    }

    public async Task<TurmaDto> ObterDadosTurmaAsync(int codigoTurma)
    {
        var httpClient = _httpClientFactory.CreateClient(ServicoEolConstantes.SERVICO);

        string urlFinal = string.Format(ServicoEolConstantes.URL_BUSCAR_TURMA, codigoTurma);
        var resposta = await httpClient.GetAsync(urlFinal);

        if (!resposta.IsSuccessStatusCode || resposta.StatusCode == HttpStatusCode.NoContent)
            return new TurmaDto();

        var json = await resposta.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
            return new TurmaDto();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<TurmaDto>(json, options);
    }

    public async Task<DadosUsuarioDto> ObterDadosUsuarioAsync(string codigoRf)
    {
        var httpClient = _httpClientFactory.CreateClient(ServicoEolConstantes.SERVICO);

        string urlFinal = string.Format(ServicoEolConstantes.URL_BUSCAR_DADOS_USUARIO, codigoRf);
        var resposta = await httpClient.GetAsync(urlFinal);

        if (!resposta.IsSuccessStatusCode || resposta.StatusCode == HttpStatusCode.NoContent)
            return new DadosUsuarioDto();

        var json = await resposta.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
            return new DadosUsuarioDto();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<DadosUsuarioDto>(json, options);
    }
}
