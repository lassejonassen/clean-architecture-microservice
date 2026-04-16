namespace CleanArchitecture.Infrastructure.Messaging.RabbitMq;

public class RabbitMqSettings
{
    public string HostName { get; init; } = "localhost";
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string ExchangeName { get; init; } = "integration_events_exchange";
}
