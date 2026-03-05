using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.Sondagem.MS.Relatorios.Aplicacao.Services;
using SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Services;
using SME.Sondagem.MS.Relatorios.IoC.Extensions;

namespace SME.Sondagem.MS.Relatorios.IoC;

public static class RegistraDependencias
{
    public static void Registrar(IServiceCollection services, IConfiguration configuration)
    {
        ConfigurarRabbitmq(services, configuration);
        ConfigurarRabbitmqLog(services, configuration);

        RegistrarRepositorios(services);
        RegistrarServicos(services, configuration);
        RegistrarCasosDeUso(services);
        RegistrarIntegracoes(services);
    }

    private static void RegistrarRepositorios(IServiceCollection services)
    {

    }

    private static void RegistrarServicos(IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<IServicoTelemetria, ServicoTelemetria>();
        services.TryAddScoped<IServicoLog, ServicoLog>();
        services.TryAddSingleton<IServicoMensageria, ServicoMensageria>();
        services.AddHttpClient();
        services.AdicionarHttpClients(configuration);

        services.TryAddScoped<IRelatorioSondagemQuestionarioPorTurmaPdf, RelatorioSondagemQuestionarioPorTurmaPdf>();
        services.TryAddScoped<IRelatorioSondagemQuestionarioPorTurmaExcel, RelatorioSondagemQuestionarioPorTurmaExcel>();

    }

    private static void RegistrarIntegracoes(IServiceCollection services)
    {
        services.AddScoped<IServicoSondagemApiClient, ServicoSondagemApiClient>();
        services.AddScoped<IServicoSgpApiClient, ServicoSgpApiClient>();
    }

    private static void RegistrarCasosDeUso(IServiceCollection services)
    {
        services.TryAddScoped<IRelatorioSondagemQuestionarioPorTurmaUseCase, RelatorioSondagemQuestionarioPorTurmaUseCase>();
    }

    private static void ConfigurarRabbitmq(IServiceCollection services, IConfiguration configuration)
    {
        var rabbitOptions = new RabbitOptions();
        configuration.GetSection(RabbitOptions.Secao).Bind(rabbitOptions, c => c.BindNonPublicProperties = true);
        services.AddSingleton(rabbitOptions);
    }

    private static void ConfigurarRabbitmqLog(IServiceCollection services, IConfiguration configuration)
    {
        var rabbitLogOptions = new RabbitLogOptions();
        configuration.GetSection(RabbitLogOptions.Secao).Bind(rabbitLogOptions, c => c.BindNonPublicProperties = true);
        services.AddSingleton(rabbitLogOptions);
    }
}
