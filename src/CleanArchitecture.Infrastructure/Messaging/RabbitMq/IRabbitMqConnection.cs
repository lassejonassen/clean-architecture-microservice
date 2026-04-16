using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using RabbitMQ.Client;

namespace CleanArchitecture.Infrastructure.Messaging.RabbitMq;

public interface IRabbitMqConnection : IDisposable
{
    bool IsConnected { get; }
    Task<IChannel> CreateChannelAsync(CancellationToken ct = default);
}

public sealed class RabbitMqConnection : IRabbitMqConnection, IDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly ResiliencePipeline _pipeline; // The resilience wrapper
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqConnection(
        RabbitMqSettings settings,
        ILogger<RabbitMqConnection> logger,
        ResiliencePipelineProvider<string> pipelineProvider) // Injected provider
    {
        _logger = logger;
        _factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            UserName = settings.UserName,
            Password = settings.Password
        };

        // Retrieve the "rabbitmq-connection" policy we will define in DI
        _pipeline = pipelineProvider.GetPipeline("rabbitmq-connection");
    }

    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    public async Task<IChannel> CreateChannelAsync(CancellationToken ct = default)
    {
        if (!IsConnected)
        {
            await TryConnectAsync(ct);
        }

        return await _connection!.CreateChannelAsync(cancellationToken: ct);
    }

    private async Task TryConnectAsync(CancellationToken ct)
    {
        // Use a lock to prevent multiple threads from creating connections simultaneously
        await _connectionLock.WaitAsync(ct);
        try
        {
            if (IsConnected) return;

            // Use the Polly pipeline to execute the connection logic with retries
            _connection = await _pipeline.ExecuteAsync(async token =>
                await _factory.CreateConnectionAsync(token), ct);

            _logger.LogInformation("Successfully established RabbitMQ connection.");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "RabbitMQ connection failed after retries.");
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connection?.Dispose();
        _connectionLock.Dispose();
    }
}