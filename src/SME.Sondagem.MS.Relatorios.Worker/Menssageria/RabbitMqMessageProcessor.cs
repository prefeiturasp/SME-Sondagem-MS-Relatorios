using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SME.Sondagem.MS.Relatorios.Dominio.Entidades;
using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using SME.Sondagem.MS.Relatorios.Infra.Exceptions;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Fila;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;
using System.Text;
using static SME.Sondagem.MS.Relatorios.Infra.Services.ServicoTelemetria;

namespace SME.Sondagem.MS.Relatorios.Worker.Menssageria;

public class RabbitMqMessageProcessor : IRabbitMqMessageProcessor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IServicoTelemetria _servicoTelemetria;
    private readonly IServicoLog _servicoLog;
    private readonly IServicoMensageria _servicoMensageria;
    private readonly ILogger<RabbitMqMessageProcessor> _logger;

    public RabbitMqMessageProcessor(
        IServiceScopeFactory serviceScopeFactory,
        IServicoTelemetria servicoTelemetria,
        IServicoLog servicoLog,
        IServicoMensageria servicoMensageria,
        ILogger<RabbitMqMessageProcessor> logger)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _servicoTelemetria = servicoTelemetria ?? throw new ArgumentNullException(nameof(servicoTelemetria));
        _servicoLog = servicoLog ?? throw new ArgumentNullException(nameof(servicoLog));
        _servicoMensageria = servicoMensageria ?? throw new ArgumentNullException(nameof(servicoMensageria));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessMessageAsync(BasicDeliverEventArgs ea, IChannel channel, Dictionary<string, ComandoRabbit> comandos)
    {
        var mensagem = Encoding.UTF8.GetString(ea.Body.ToArray());
        LogMensagemRecebida(mensagem);
        var rota = ea.RoutingKey;

        if (!comandos.TryGetValue(rota, out var comandoRabbit))
        {
            await channel.BasicRejectAsync(ea.DeliveryTag, false);
            return;
        }

        var transacao = _servicoTelemetria.IniciarTransacao(rota);
        var mensagemRabbit = mensagem.ConverterObjectStringPraObjeto<MensagemRabbit>();

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var casoDeUso = scope.ServiceProvider.GetService(comandoRabbit.TipoCasoUso)
                ?? throw new ArgumentNullException(comandoRabbit.TipoCasoUso.Name);

            if (mensagemRabbit is not null)
            {
                await ExecutarCasoDeUsoAsync(comandoRabbit, casoDeUso, mensagemRabbit, rota);
            }

            await channel.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (NegocioException nex)
        {
            if (mensagemRabbit is not null)
                await HandleNegocioExceptionAsync(ea, channel, mensagemRabbit, nex, transacao);
        }
        catch (Exception ex)
        {
            if (mensagemRabbit is not null)
                await HandleExceptionAsync(ea, channel, mensagemRabbit, comandoRabbit, ex, transacao);
        }
        finally
        {
            _servicoTelemetria.FinalizarTransacao(transacao);
        }
    }

    private void LogMensagemRecebida(string mensagem)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("MensagemRecebida: {MensagemRecebida}", mensagem);
        }
    }

    private async Task ExecutarCasoDeUsoAsync(ComandoRabbit comandoRabbit, object casoDeUso, MensagemRabbit mensagemRabbit, string rota)
    {
        var metodo = (comandoRabbit.TipoCasoUso?.ObterMetodo("Executar")) ?? throw new InvalidOperationException($"O método 'Executar' não foi encontrado em {comandoRabbit.TipoCasoUso?.FullName ?? "tipo desconhecido"}.");
        await _servicoTelemetria.RegistrarAsync(() =>
            metodo.InvokeAsync(casoDeUso, mensagemRabbit),
            "RabbitMq",
            rota,
            rota);
    }

    private async Task HandleNegocioExceptionAsync(BasicDeliverEventArgs ea, IChannel channel, MensagemRabbit mensagemRabbit, NegocioException nex, ServicoTelemetriaTransacao transacao)
    {
        await channel.BasicAckAsync(ea.DeliveryTag, false);

        if (mensagemRabbit != null)
            RegistrarLog(ea, mensagemRabbit, nex, LogNivel.Negocio, $"Erros: {nex.Message}");

        _servicoTelemetria.RegistrarExcecao(transacao, nex);
    }

    private async Task HandleExceptionAsync(BasicDeliverEventArgs ea, IChannel channel, MensagemRabbit mensagemRabbit, ComandoRabbit comandoRabbit, Exception ex, ServicoTelemetriaTransacao transacao)
    {
        _servicoTelemetria.RegistrarExcecao(transacao, ex);
        var rejeicoes = GetRetryCount(ea.BasicProperties);

        if (rejeicoes + 1 >= comandoRabbit.QuantidadeReprocessamentoDeadLetter)
        {
            await channel.BasicAckAsync(ea.DeliveryTag, false);

            var filaFinal = $"{ea.RoutingKey}.deadletter.final";

            if (mensagemRabbit != null)
                await _servicoMensageria.Publicar(mensagemRabbit, filaFinal, ExchangeRabbit.SgpDeadLetter, "PublicarDeadLetter");
        }
        else
        {
            await channel.BasicRejectAsync(ea.DeliveryTag, false);
        }

        if (mensagemRabbit != null)
            RegistrarLog(ea, mensagemRabbit, ex, LogNivel.Critico, $"Erros: {ex.Message}");
    }

    private static ulong GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers is null ||
            !properties.Headers.TryGetValue("x-death", out var xDeath) ||
            xDeath is not IList<object> deathProperties ||
            deathProperties.Count == 0)
        {
            return 0;
        }

        if (deathProperties[0] is not IDictionary<string, object> lastRetry ||
            !lastRetry.TryGetValue("count", out var count))
        {
            return 0;
        }

        return (ulong)Convert.ToInt64(count);
    }

    private void RegistrarLog(BasicDeliverEventArgs ea, MensagemRabbit mensagemRabbit, Exception ex, LogNivel logNivel, string observacao)
    {
        var mensagem = $"Worker Abrangencia: Rota -> {ea.RoutingKey}  Cod Correl -> {mensagemRabbit.CodigoCorrelacao.ToString()[..3]}";

        var logMensagem = new LogMensagem(mensagem, logNivel, observacao, ex?.StackTrace ?? string.Empty, ex?.InnerException?.Message ?? string.Empty);

        var exceptionToLog = new Exception(logMensagem.Mensagem, ex);

        _servicoLog.Registrar(exceptionToLog);
    }
}