using RabbitMQ.Client;

namespace FiscalFlow.Infrastructure.RabbitMq;

public sealed class RabbitMqTopologyInitializer
{
    private readonly RabbitMqConnectionFactory
        _connectionFactory;

    private readonly RabbitMqOptions _options;

    public RabbitMqTopologyInitializer(
        RabbitMqConnectionFactory connectionFactory,
        RabbitMqOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            connectionFactory);

        ArgumentNullException.ThrowIfNull(options);

        _connectionFactory = connectionFactory;
        _options = options;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            await _connectionFactory.GetConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        await DeclareExchangesAsync(
            channel,
            cancellationToken);

        await DeclareDeadLetterQueueAsync(
            channel,
            cancellationToken);

        await DeclareRetryQueueAsync(
            channel,
            cancellationToken);

        await DeclareMainQueueAsync(
            channel,
            cancellationToken);
    }

    private async Task DeclareExchangesAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.RetryExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    private async Task DeclareDeadLetterQueueAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.QueueDeclareAsync(
            queue: _options.DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.DeadLetterQueueName,
            exchange: _options.DeadLetterExchangeName,
            routingKey:
                _options.DeadLetterRoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    private async Task DeclareRetryQueueAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        var arguments =
            new Dictionary<string, object?>
            {
                ["x-message-ttl"] =
                    _options.RetryDelayMilliseconds,

                ["x-dead-letter-exchange"] =
                    _options.ExchangeName,

                ["x-dead-letter-routing-key"] =
                    _options.RoutingKey
            };

        await channel.QueueDeclareAsync(
            queue: _options.RetryQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.RetryQueueName,
            exchange: _options.RetryExchangeName,
            routingKey: _options.RetryRoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    private async Task DeclareMainQueueAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        var arguments =
            new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] =
                    _options.RetryExchangeName,

                ["x-dead-letter-routing-key"] =
                    _options.RetryRoutingKey
            };

        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.QueueName,
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);
    }
}