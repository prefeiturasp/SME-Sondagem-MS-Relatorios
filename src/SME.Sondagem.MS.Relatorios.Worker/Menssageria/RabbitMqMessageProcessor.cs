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
        _logger.LogInformation("Mensagem recebida: {mensagem}", mensagem);
        var rota = ea.RoutingKey;

        if (!comandos.ContainsKey(rota))
        {
            await channel.BasicRejectAsync(ea.DeliveryTag, false);
            return;
        }

        var transacao = _servicoTelemetria.IniciarTransacao(rota);
        var mensagemRabbit = mensagem.ConverterObjectStringPraObjeto<MensagemRabbit>();
        var comandoRabbit = comandos[rota];

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var casoDeUso = scope.ServiceProvider.GetService(comandoRabbit.TipoCasoUso);

            if (casoDeUso == null)
                throw new ArgumentNullException(comandoRabbit.TipoCasoUso.Name);

            await _servicoTelemetria.RegistrarAsync(() =>
                comandoRabbit.TipoCasoUso.ObterMetodo("Executar").InvokeAsync(casoDeUso, mensagemRabbit),
                "RabbitMq",
                rota,
                rota);

            await channel.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (NegocioException nex)
        {
            _logger.LogError("Error: {0}", nex);
            await channel.BasicAckAsync(ea.DeliveryTag, false);
            RegistrarLog(ea, mensagemRabbit, nex, LogNivel.Negocio, $"Erros: {nex.Message}");
            _servicoTelemetria.RegistrarExcecao(transacao, nex);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: {0}", ex);
            _servicoTelemetria.RegistrarExcecao(transacao, ex);
            var rejeicoes = GetRetryCount(ea.BasicProperties);

            if (++rejeicoes >= comandoRabbit.QuantidadeReprocessamentoDeadLetter)
            {
                await channel.BasicAckAsync(ea.DeliveryTag, false);

                var filaFinal = $"{ea.RoutingKey}.deadletter.final";

                await _servicoMensageria.Publicar(mensagemRabbit, filaFinal,
                    ExchangeRabbit.SgpDeadLetter,
                    "PublicarDeadLetter");
            }
            else
            {
                await channel.BasicRejectAsync(ea.DeliveryTag, false);
            }

            RegistrarLog(ea, mensagemRabbit, ex, LogNivel.Critico, $"Erros: {ex.Message}");
        }
        finally
        {
            _servicoTelemetria.FinalizarTransacao(transacao);
        }
    }

    private ulong GetRetryCount(IReadOnlyBasicProperties properties)
    {
        if (properties.Headers == null || !properties.Headers.ContainsKey("x-death"))
            return 0;

        var deathProperties = (List<object>)properties.Headers["x-death"];
        if (deathProperties.Count == 0)
            return 0;

        var lastRetry = (Dictionary<string, object>)deathProperties[0];

        if (!lastRetry.ContainsKey("count"))
            return 0;

        var count = lastRetry["count"];

        return (ulong)Convert.ToInt64(count);
    }

    private void RegistrarLog(BasicDeliverEventArgs ea, MensagemRabbit mensagemRabbit, Exception ex, LogNivel logNivel, string observacao)
    {
        var mensagem = $"Worker Abrangencia: Rota -> {ea.RoutingKey}  Cod Correl -> {mensagemRabbit.CodigoCorrelacao.ToString()[..3]}";

        var logMensagem = new LogMensagem(mensagem, logNivel, observacao, ex?.StackTrace, ex?.InnerException?.Message);

        var exceptionToLog = new Exception(logMensagem.Mensagem, ex);

        _servicoLog.Registrar(exceptionToLog);
    }
}