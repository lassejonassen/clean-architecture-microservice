using CleanArchitecture.SharedKernel.IntegrationEvents;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CleanArchitecture.Infrastructure.Messaging.RabbitMq;

public class RabbitMqBus(IRabbitMqConnection connection, RabbitMqSettings settings) : IIntegrationEventBus
{
    private const string ExchangeName = "integration_events_exchange";

    public async Task SendAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class, IIntegrationEvent
    {
        using var channel = await connection.CreateChannelAsync(ct);

        // v7+ uses Async methods for topology management
        await channel.ExchangeDeclareAsync(
            exchange: settings.ExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            cancellationToken: ct);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(integrationEvent));

        // Create properties via the channel in v7+
        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Type = typeof(T).Name
        };

        // Note the slightly different signature in the v7.x client
        await channel.BasicPublishAsync(
             exchange: settings.ExchangeName,
             routingKey: string.Empty,
             mandatory: true,
             basicProperties: props,
             body: body,
             cancellationToken: ct);
    }
}