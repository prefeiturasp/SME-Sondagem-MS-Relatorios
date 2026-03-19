using RabbitMQ.Client;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.EnvironmentVariables;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Fila;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SME.Sondagem.MS.Relatorios.Infra.Services;

public class ServicoLog : IServicoLog
{
    private readonly ILogger<ServicoLog> logger;
    private readonly IServicoTelemetria servicoTelemetria;
    private readonly RabbitLogOptions configuracaoRabbitOptions;
    public ServicoLog(IServicoTelemetria servicoTelemetria, RabbitLogOptions configuracaoRabbitOptions, ILogger<ServicoLog> logger)
    {
        this.servicoTelemetria = servicoTelemetria ?? throw new ArgumentNullException(nameof(servicoTelemetria));
        this.configuracaoRabbitOptions = configuracaoRabbitOptions ?? throw new System.ArgumentNullException(nameof(configuracaoRabbitOptions));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Registrar(Exception ex)
    {
        LogMensagem logMensagem = new("Exception --- ", LogNivel.Critico, ex.Message, ex.StackTrace ?? string.Empty);
        Registrar(logMensagem);
    }

    public void Registrar(LogNivel nivel, string erro, string observacoes, string stackTrace)
    {
        LogMensagem logMensagem = new(erro, nivel, observacoes, stackTrace);
        Registrar(logMensagem);

    }

    public void Registrar(string mensagem, Exception ex)
    {
        LogMensagem logMensagem = new(mensagem, LogNivel.Critico, ex.Message, ex.StackTrace ?? string.Empty);

        Registrar(logMensagem);
    }
    private void Registrar(LogMensagem log)
    {
        var body = Encoding.UTF8.GetBytes(log.ConverterObjectParaJson());
        servicoTelemetria.Registrar(async () => await PublicarMensagem(body), "RabbitMQ", "Salvar Log Via Rabbit", RotasRabbit.RotaLogs);
    }

    private async Task PublicarMensagem(byte[] body)
    {
        try
        {
            string? userName = configuracaoRabbitOptions.UserName;
            var factory = new ConnectionFactory
            {
                HostName = configuracaoRabbitOptions?.HostName ?? string.Empty,
                UserName = userName ?? string.Empty,
                Password = configuracaoRabbitOptions?.Password ?? string.Empty,
                VirtualHost = configuracaoRabbitOptions?.VirtualHost ?? string.Empty
            };

            using var conexaoRabbit = await factory.CreateConnectionAsync();
            using var channel = await conexaoRabbit.CreateChannelAsync();
            var props = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(
                ExchangeRabbit.Sgp,
                RotasRabbit.RotaLogs,
                true,
                props,
                body
            );
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Ocorreu um erro ao tentar publicar uma mensagem no RabbitMQ.");
        }
    }
}