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

        _comandos = new Dictionary<string, ComandoRabbit>();
        RegistrarUseCases();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var conexaoRabbit = await _rabbitMqSetupService.CreateConnectionAsync(stoppingToken);
        await using var channel = await conexaoRabbit.CreateChannelAsync(null, stoppingToken);

        await _rabbitMqSetupService.SetupExchangesAndQueuesAsync(channel, _comandos);

        await InicializaConsumerAsync(channel, stoppingToken);
    }

    private void RegistrarUseCases()
    {
        _comandos.Add(RotasRabbit.RelatorioSondagemPorTurma, new ComandoRabbit("Relatorio Sondagem Por Turma", typeof(IRelatorioSondagemQuestionarioPorTurmaUseCase)));
    }

    private async Task InicializaConsumerAsync(IChannel channel, CancellationToken stoppingToken)
    {
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
                await channel.BasicRejectAsync(ea.DeliveryTag, false);
            }
        };

        await RegistrarConsumerAsync(consumer, channel);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker ativo em: {Now}", DateTime.Now);
            }
            await Task.Delay(10000, stoppingToken);
        }
    }

    private static async Task RegistrarConsumerAsync(AsyncEventingBasicConsumer consumer, IChannel channel)
    {
        var filas = typeof(RotasRabbit).ObterConstantesPublicas<string>()
            .Where(fila => !string.IsNullOrEmpty(fila));

        // S3267 fix: Use Where LINQ method instead of manual null check in loop
        foreach (var fila in filas)
        {
            await channel.BasicConsumeAsync(fila, false, consumer);
        }
    }
}