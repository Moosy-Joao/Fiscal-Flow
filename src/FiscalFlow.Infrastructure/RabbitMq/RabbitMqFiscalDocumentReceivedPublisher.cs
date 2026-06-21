using System.Diagnostics;
using System.Text.Json;
using FiscalFlow.Application.Messaging;
using FiscalFlow.Application.Observability;
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

        using var activity =
            FiscalFlowTelemetry.ActivitySource.StartActivity(
                "rabbitmq publish fiscal-document.received",
                ActivityKind.Producer);

        activity?.SetTag(
            "messaging.system",
            "rabbitmq");
        activity?.SetTag(
            "messaging.destination.name",
            _options.ExchangeName);
        activity?.SetTag(
            "messaging.operation.type",
            "publish");
        activity?.SetTag(
            "fiscal.document.id",
            message.DocumentId);

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

        var correlationId =
            activity?.GetBaggageItem("correlation.id")
            ?? message.CorrelationId.ToString();

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            Persistent = true,
            MessageId = message.DocumentId.ToString(),
            CorrelationId = correlationId,
            Type = "fiscal-document.received",
            AppId = "fiscalflow-api"
        };

        RabbitMqTraceContext.Inject(
            properties,
            activity,
            correlationId);

        try
        {
            await channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception exception)
        {
            activity?.SetStatus(
                ActivityStatusCode.Error,
                exception.Message);

            activity?.SetTag(
                "error.type",
                exception.GetType().FullName);

            throw;
        }
    }
}
