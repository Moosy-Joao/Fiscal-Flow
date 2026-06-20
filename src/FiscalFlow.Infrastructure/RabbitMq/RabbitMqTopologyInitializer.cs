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

        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.QueueName,
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);
    }
}