using CleanArchitecture.Infrastructure.Messaging.RabbitMq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchitecture.Infrastructure.BackgroundServices;

public class IntegrationEventConsumerWorker : BackgroundService
{
    private readonly IRabbitMqConnection _connection;
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<IntegrationEventConsumerWorker> _logger;
    private IChannel? _channel;
    private const string QueueName = "template_service_integration_queue";

    public IntegrationEventConsumerWorker(
        IRabbitMqConnection connection,
        RabbitMqSettings settings,
        ILogger<IntegrationEventConsumerWorker> logger)
    {
        _connection = connection;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. Ensure we have a channel
        _channel = await _connection.CreateChannelAsync(stoppingToken);

        // 2. Declare the Exchange (Just in case it doesn't exist yet)
        await _channel.ExchangeDeclareAsync(
            exchange: _settings.ExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            cancellationToken: stoppingToken);

        // 3. Declare a Queue for THIS specific service
        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // 4. Bind the Queue to the Exchange
        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: _settings.ExchangeName,
            routingKey: string.Empty,
            cancellationToken: stoppingToken);

        // 5. Set up the Consumer
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Integration Event Received: {Message}", message);

                // TODO: Here you would deserialize and call a service/handler
                // var @event = JsonSerializer.Deserialize<YourEvent>(message);

                // 6. Acknowledge the message (Remove it from the queue)
                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing integration event");
                // Reject and requeue so it can be retried
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
            }
        };

        // 7. Start Consuming
        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false, // We manually Ack to ensure reliability
            consumer: consumer,
            cancellationToken: stoppingToken);

        // Keep the service alive until stopped
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}