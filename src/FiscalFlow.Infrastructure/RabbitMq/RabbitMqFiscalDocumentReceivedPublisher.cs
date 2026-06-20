using System.Text.Json;
using FiscalFlow.Application.Messaging;
using RabbitMQ.Client;

namespace FiscalFlow.Infrastructure.RabbitMq;

public sealed class RabbitMqFiscalDocumentReceivedPublisher :
    IFiscalDocumentReceivedPublisher
{
    private static readonly JsonSerializerOptions
        SerializerOptions =
            new(JsonSerializerDefaults.Web);

    private readonly RabbitMqConnectionFactory
        _connectionFactory;

    private readonly RabbitMqOptions _options;

    public RabbitMqFiscalDocumentReceivedPublisher(
        RabbitMqConnectionFactory connectionFactory,
        RabbitMqOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            connectionFactory);

        ArgumentNullException.ThrowIfNull(options);

        _connectionFactory = connectionFactory;
        _options = options;
    }

    public async Task PublishAsync(
        FiscalDocumentReceivedMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var body =
            JsonSerializer.SerializeToUtf8Bytes(
                message,
                SerializerOptions);

        var connection =
            await _connectionFactory.GetConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            Persistent = true,
            MessageId = message.DocumentId.ToString(),
            CorrelationId =
                message.CorrelationId.ToString(),
            Type = "fiscal-document.received",
            AppId = "fiscalflow-api"
        };

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: _options.RoutingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}