using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SME.Sondagem.MS.Relatorios.Aplicacao.Services;
using SME.Sondagem.MS.Relatorios.Aplicacao.UseCases;
using SME.Sondagem.MS.Relatorios.Dados.Repositorios;
using SME.Sondagem.MS.Relatorios.Dominio.Interfaces;
using SME.Sondagem.MS.Relatorios.Excel.Interfaces;
using SME.Sondagem.MS.Relatorios.Excel.Templates;
using SME.Sondagem.MS.Relatorios.HtmlPdf;
using SME.Sondagem.MS.Relatorios.HtmlPdf.Interfaces;
using SME.Sondagem.MS.Relatorios.HtmlPdf.Templates;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using SME.Sondagem.MS.Relatorios.Infra.Services;
using SME.Sondagem.MS.Relatorios.IoC.Extensions;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace SME.Sondagem.MS.Relatorios.IoC;

public static class RegistraDependencias
{
    public static void Registrar(IServiceCollection services, IConfiguration configuration)
    {
        ConfigurarRabbitmq(services, configuration);
        ConfigurarRabbitmqLog(services, configuration);

        var context = new CustomAssemblyLoadContext();
        var nomeBliblioteca = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) ? "libwkhtmltox.dll" : "libwkhtmltox.so";
        context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), nomeBliblioteca));

        RegistrarServicos(services, configuration);
        RegistrarCasosDeUso(services);
        RegistrarIntegracoes(services);
    }

    private static void RegistrarServicos(IServiceCollection services, IConfiguration configuration)
    {
        var telemetria = new TelemetriaOptions();
        configuration.GetSection(TelemetriaOptions.Secao).Bind(telemetria, c => c.BindNonPublicProperties = true);
        services.AddSingleton(telemetria);

        // or reuse IOptions<T>:
        services.AddSingleton(provider => provider.GetRequiredService<IOptions<TelemetriaOptions>>().Value);

        services.TryAddScoped<IServicoTelemetria, ServicoTelemetria>();
        services.TryAddScoped<IRepositorioRacaCor, RepositorioRacaCor>();
        services.TryAddScoped<IRepositorioGeneroSexo, RepositorioGeneroSexo>();
        services.TryAddScoped<IRepositorioBimestre, RepositorioBimestre>();
        services.TryAddScoped<IRepositorioComponenteCurricular, RepositorioComponenteCurricular>();
        services.TryAddScoped<IServicoLog, ServicoLog>();
        services.TryAddSingleton<IServicoMensageria, ServicoMensageria>();
        services.AddHttpClient();
        services.AdicionarHttpClients(configuration);

        services.TryAddScoped<IRelatorioSondagemQuestionarioPorTurmaPdf, RelatorioSondagemQuestionarioPorTurmaPdf>();
        services.TryAddScoped<IRelatorioSondagemQuestionarioPorTurmaExcel, RelatorioSondagemQuestionarioPorTurmaExcel>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoRacaPdf, RelatorioSondagemConsolidadoRacaPdf>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoGeneroPdf, RelatorioSondagemConsolidadoGeneroPdf>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoRacaGeneroPdf, RelatorioSondagemConsolidadoRacaGeneroPdf>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoBimestrePdf, RelatorioSondagemConsolidadoBimestrePdf>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoQuestaoPdf, RelatorioSondagemConsolidadoQuestaoPdf>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoGenericoExcel, RelatorioSondagemConsolidadoGenericoExcel>();

        services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
        services.TryAddScoped<IReportConverter, ReportConverter>();
        services.TryAddScoped<IServicoArmazenamentoMinio, ServicoArmazenamentoMinio>();
        services.TryAddScoped<IRelatorioSondagemQuestionarioPorTurmaTemplatePdf, RelatorioSondagemQuestionarioPorTurmaTemplatePdf>();
        services.TryAddScoped<IRelatorioSondagemQuestionarioPorTurmaTemplateExcel, RelatorioSondagemQuestionarioPorTurmaTemplateExcel>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoPorRacaTemplatePdf, RelatorioSondagemConsolidadoPorRacaTemplatePdf>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoPorGeneroTemplatePdf, RelatorioSondagemConsolidadoPorGeneroTemplatePdf>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoPorRacaGeneroTemplatePdf, RelatorioSondagemConsolidadoPorRacaGeneroTemplatePdf>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoPorBimestreTemplatePdf, RelatorioSondagemConsolidadoPorBimestreTemplatePdf>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoPorQuestaoTemplatePdf, RelatorioSondagemConsolidadoPorQuestaoTemplatePdf>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoGenericoTemplateExcel, RelatorioSondagemConsolidadoGenericoTemplateExcel>();

    }

    private static void RegistrarIntegracoes(IServiceCollection services)
    {
        services.AddScoped<IServicoSondagemApiClient, ServicoSondagemApiClient>();
        services.AddScoped<IServicoSgpApiClient, ServicoSgpApiClient>();
        services.AddScoped<IServicoEolApiClient, ServicoEolApiClient>();
    }

    private static void RegistrarCasosDeUso(IServiceCollection services)
    {
        services.TryAddScoped<IRelatorioSondagemQuestionarioPorTurmaUseCase, RelatorioSondagemQuestionarioPorTurmaUseCase>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoRacaUseCase, RelatorioSondagemConsolidadoRacaUseCase>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoGeneroUseCase, RelatorioSondagemConsolidadoGeneroUseCase>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoRacaGeneroUseCase, RelatorioSondagemConsolidadoRacaGeneroUseCase>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoBimestreUseCase, RelatorioSondagemConsolidadoBimestreUseCase>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoPorBimestreUseCase, RelatorioSondagemConsolidadoPorBimestreUseCase>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoQuestaoUseCase, RelatorioSondagemConsolidadoQuestaoUseCase>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoRacaGeneroUseCase, RelatorioSondagemConsolidadoRacaGeneroUseCase>();
        services.TryAddScoped<IRelatorioSondagemConsolidadoAnoUseCase, RelatorioSondagemConsolidadoAnoUseCase>();
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
