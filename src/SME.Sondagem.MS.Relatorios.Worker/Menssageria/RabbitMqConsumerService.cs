using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SME.Sondagem.MS.Relatorios.Infra.Extensions;
using SME.Sondagem.MS.Relatorios.Infra.Fila;
using SME.Sondagem.MS.Relatorios.Infra.Interfaces;

namespace SME.Sondagem.MS.Relatorios.Worker.Menssageria;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly IServicoLog _servicoLog;
    private readonly IRabbitMqSetupService _rabbitMqSetupService;
    private readonly IRabbitMqMessageProcessor _rabbitMqMessageProcessor;

    private readonly Dictionary<string, ComandoRabbit> _comandos;

    public RabbitMqConsumerService(
        ILogger<RabbitMqConsumerService> logger,
        IServicoLog servicoLog,
        IServicoMensageria servicoMensageria,
        IRabbitMqSetupService rabbitMqSetupService,
        IRabbitMqMessageProcessor rabbitMqMessageProcessor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _servicoLog = servicoLog ?? throw new ArgumentNullException(nameof(servicoLog));

        _rabbitMqSetupService = rabbitMqSetupService ?? throw new ArgumentNullException(nameof(rabbitMqSetupService));
        _rabbitMqMessageProcessor = rabbitMqMessageProcessor ?? throw new ArgumentNullException(nameof(rabbitMqMessageProcessor));

        _comandos = [];
        RegistrarUseCases();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var conexaoRabbit = await _rabbitMqSetupService.CreateConnectionAsync(stoppingToken);
                await using var channel = await conexaoRabbit.CreateChannelAsync(null, stoppingToken);

                await _rabbitMqSetupService.SetupExchangesAndQueuesAsync(channel, _comandos);

                await InicializaConsumerAsync(channel, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Conexão RabbitMQ perdida. Reconectando em 5s...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private void RegistrarUseCases()
    {
        _comandos.Add(RotasRabbit.RelatorioSondagemPorTurma, new ComandoRabbit("Relatorio Sondagem Por Turma", typeof(IRelatorioSondagemQuestionarioPorTurmaUseCase)));
        _comandos.Add(RotasRabbit.RelatorioSondagemConsolidadoPorRaca, new ComandoRabbit("Relatorio Sondagem Consolidado Por Raca", typeof(IRelatorioSondagemConsolidadoRacaUseCase)));
        _comandos.Add(RotasRabbit.RelatorioSondagemConsolidadoPorGenero, new ComandoRabbit("Relatorio Sondagem Consolidado Por Genero", typeof(IRelatorioSondagemConsolidadoGeneroUseCase)));
        _comandos.Add(RotasRabbit.RelatorioSondagemConsolidadoPorRacaGenero, new ComandoRabbit("Relatorio Sondagem Consolidado Por Raca e Genero", typeof(IRelatorioSondagemConsolidadoRacaGeneroUseCase), ttl: ExchangeRabbit.SgpDeadLetterTTL_3));
        _comandos.Add(RotasRabbit.RelatorioSondagemConsolidadoPorAno, new ComandoRabbit("Relatorio Sondagem Consolidado Por Ano", typeof(IRelatorioSondagemConsolidadoQuestaoUseCase), ttl: ExchangeRabbit.SgpDeadLetterTTL_3));
        _comandos.Add(RotasRabbit.RelatorioSondagemConsolidadoPorBimestre, new ComandoRabbit("Relatorio Sondagem Consolidado Por Bimestre", typeof(IRelatorioSondagemConsolidadoPorBimestreUseCase), ttl: ExchangeRabbit.SgpDeadLetterTTL_3));
    }

    private async Task InicializaConsumerAsync(IChannel channel, CancellationToken stoppingToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        channel.ChannelShutdownAsync += (sender, args) =>
        {
            cts.Cancel();
            return Task.CompletedTask;
        };

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                await _rabbitMqMessageProcessor.ProcessMessageAsync(ea, channel, _comandos);
            }
            catch (Exception ex)
            {
                _servicoLog.Registrar($"Erro ao tratar mensagem {ea.DeliveryTag}", ex);
                try { await channel.BasicRejectAsync(ea.DeliveryTag, false); }
                catch (Exception rejectEx)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogError(rejectEx, "Falha ao rejeitar mensagem {DeliveryTag}", ea.DeliveryTag);
                }
            }
        };

        await RegistrarConsumerAsync(consumer, channel);

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Worker ativo em: {Now}", DateTime.Now);

                await Task.Delay(10000, cts.Token);
            }
        }
        catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("Canal RabbitMQ fechado inesperadamente.");
        }
    }

    private static async Task RegistrarConsumerAsync(AsyncEventingBasicConsumer consumer, IChannel channel)
    {
        var filas = typeof(RotasRabbit).ObterConstantesPublicas<string>()
            .Where(fila => !string.IsNullOrEmpty(fila));

        foreach (var fila in filas)
        {
            if (fila == null) continue;
            await channel.BasicConsumeAsync(fila, false, consumer);
        }
    }
}