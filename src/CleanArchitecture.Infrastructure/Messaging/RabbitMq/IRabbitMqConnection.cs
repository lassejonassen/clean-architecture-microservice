using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CleanArchitecture.Infrastructure.Messaging.RabbitMq;

public interface IRabbitMqConnection : IDisposable
{
    bool IsConnected { get; }
    Task<IChannel> CreateChannelAsync(CancellationToken ct = default);
}

public sealed class RabbitMqConnection : IRabbitMqConnection
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMqConnection> _logger;
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqConnection(
        RabbitMqSettings settings,
        ILogger<RabbitMqConnection> logger)
    {
        _logger = logger;
        _factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            UserName = settings.UserName,
            Password = settings.Password,
            // DispatchConsumersAsync is gone; the client is now async by default.
        };
    }

    public bool IsConnected => _connection is not null && !_disposed;

    public async Task<IChannel> CreateChannelAsync(CancellationToken ct = default)
    {
        if (!IsConnected)
        {
            await TryConnectAsync(ct);
        }

        if (_connection is null)
        {
            throw new InvalidOperationException("RabbitMQ connection could not be established.");
        }

        // In v7+, CreateChannelAsync is the preferred method
        return await _connection.CreateChannelAsync(cancellationToken: ct);
    }

    private async Task TryConnectAsync(CancellationToken ct)
    {
        try
        {
            // Must use 'await' here
            _connection = await _factory.CreateConnectionAsync(ct);
            _logger.LogInformation("Connected to RabbitMQ");
        }
        catch (Exception ex)
        {
            // This is where "None of the specified endpoints were reachable" is caught
            _logger.LogError(ex, "Failed to connect to RabbitMQ at {Host}", _factory.HostName);
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connection?.Dispose();
    }
}