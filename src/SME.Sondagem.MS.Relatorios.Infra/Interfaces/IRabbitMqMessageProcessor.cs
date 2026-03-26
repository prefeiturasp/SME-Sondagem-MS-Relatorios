using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SME.Sondagem.MS.Relatorios.Infra.Fila;

namespace SME.Sondagem.MS.Relatorios.Infra.Interfaces;

public interface IRabbitMqMessageProcessor
{
    Task ProcessMessageAsync(BasicDeliverEventArgs ea, IChannel channel, Dictionary<string, ComandoRabbit> comandos);
}
