using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SME.Sondagem.MS.Relatorios.Infra.Constantes;

namespace SME.Sondagem.MS.Relatorios.IoC.Extensions;

internal static class RegistrarHttpClients
{
    internal static void AdicionarHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigurarHttpClientSgp(configuration);
        services.ConfigurarHttpClientSondagem(configuration);
    }


    private static void ConfigurarHttpClientSgp(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration.ValidarConfiguracao("UrlApiSGP");
        var apiKey = configuration.GetValue<string>("ApiKeySGPApi");

        services.AddHttpClient("ApiSGP", client =>
        {
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("x-sgp-api-key", apiKey);
        });

        services.AddHttpClient(ServicoSgpConstantes.SERVICO, client =>
        {
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("x-sgp-api-key", apiKey);
        });
    }

    private static void ConfigurarHttpClientSondagem(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration.ValidarConfiguracao("UrlApiSondagem");
        var apiKey = configuration.GetValue<string>("ApiKeySondagemApi");

        services.AddHttpClient("ApiSondagem", client =>
        {
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("x-api-sondagem-key", apiKey);
        });

        services.AddHttpClient(ServicoSondagemConstantes.SERVICO, client =>
        {
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("x-api-sondagem-key", apiKey);
        });
    }

    private static string ValidarConfiguracao(this IConfiguration configuration, string chave)
    {
        var valor = configuration.GetValue<string>(chave);
        if (string.IsNullOrWhiteSpace(valor))
            throw new InvalidOperationException($"A configuração '{chave}' é obrigatória.");

        return valor;
    }
}