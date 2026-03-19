using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nest;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;

namespace SME.Sondagem.MS.Relatorios.IoC;

public static class ConfigureServices
{
    public static void ConfigurarServicos(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConnectionStringOptions>(configuration.GetSection(ConnectionStringOptions.Secao));
        services.Configure<RabbitOptions>(configuration.GetSection(RabbitOptions.Secao));
        services.Configure<RabbitLogOptions>(configuration.GetSection(RabbitLogOptions.Secao));
        services.Configure<TelemetriaOptions>(configuration.GetSection(TelemetriaOptions.Secao));
        services.Configure<ElasticOptions>(configuration.GetSection(ElasticOptions.Secao));

        services.AddSingleton<IElasticClient>(provider =>
        {
            var elasticOptions = provider.GetRequiredService<IOptions<ElasticOptions>>().Value;
            var nodes = elasticOptions?.Urls?.Split(',').Select(url => new Uri(url)).ToList();

            var connectionPool = new StaticConnectionPool(nodes);
            var connectionSettings = new ConnectionSettings(connectionPool)
                .DefaultIndex(elasticOptions?.IndicePadrao);

            if (!string.IsNullOrEmpty(elasticOptions?.CertificateFingerprint))
                connectionSettings.CertificateFingerprint(elasticOptions?.CertificateFingerprint);

            if (!string.IsNullOrEmpty(elasticOptions?.Usuario) && !string.IsNullOrEmpty(elasticOptions?.Senha))
            {
                connectionSettings?.BasicAuthentication(elasticOptions.Usuario, elasticOptions.Senha);
            }

            return new ElasticClient(connectionSettings);
        });
    }

    public static void ConfigurarConexoes(IServiceCollection services, IConfiguration configuration)
    {
        var connectionStringOptions = new ConnectionStringOptions();
        configuration.GetSection(ConnectionStringOptions.Secao).Bind(connectionStringOptions, c => c.BindNonPublicProperties = true);
        services.AddSingleton(connectionStringOptions);
    }
}
