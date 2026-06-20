using RabbitMQ.Client;

using ClientConnectionFactory =
    RabbitMQ.Client.ConnectionFactory;

namespace FiscalFlow.Infrastructure.RabbitMq;

public sealed class RabbitMqConnectionFactory :
    IAsyncDisposable
{
    private readonly ClientConnectionFactory _factory;

    private readonly SemaphoreSlim _connectionLock =
        new(1, 1);

    private IConnection? _connection;

    public RabbitMqConnectionFactory(
        RabbitMqOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            options.HostName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            options.UserName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            options.Password);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            options.VirtualHost);

        if (options.Port <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options.Port),
                "A porta do RabbitMQ deve ser maior que zero.");
        }

        _factory = new ClientConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            VirtualHost = options.VirtualHost,

            ClientProvidedName = "fiscalflow-api",

            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,

            NetworkRecoveryInterval =
                TimeSpan.FromSeconds(5),

            RequestedHeartbeat =
                TimeSpan.FromSeconds(30)
        };
    }

    public async Task<IConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _connectionLock.WaitAsync(
            cancellationToken);

        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }

            _connection =
                await _factory.CreateConnectionAsync(
                    cancellationToken);

            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connectionLock.WaitAsync();

        try
        {
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        finally
        {
            _connectionLock.Release();
            _connectionLock.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}