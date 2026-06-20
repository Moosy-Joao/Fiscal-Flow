using System.Text.Json;
using FiscalFlow.Application.Messaging;
using FiscalFlow.Infrastructure.RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FiscalFlow.Api.Messaging;

public sealed class FiscalDocumentReceivedConsumer :
    BackgroundService
{
    private static readonly JsonSerializerOptions
        SerializerOptions =
            new(JsonSerializerDefaults.Web);

    private readonly RabbitMqConnectionFactory
        _connectionFactory;

    private readonly RabbitMqOptions _options;

    private readonly ILogger<
        FiscalDocumentReceivedConsumer> _logger;

    private IChannel? _channel;

    public FiscalDocumentReceivedConsumer(
        RabbitMqConnectionFactory connectionFactory,
        RabbitMqOptions options,
        ILogger<FiscalDocumentReceivedConsumer> logger)
    {
        ArgumentNullException.ThrowIfNull(
            connectionFactory);

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionFactory = connectionFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var connection =
            await _connectionFactory.GetConnectionAsync(
                stoppingToken);

        _channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Consumidor RabbitMQ iniciado na fila {QueueName}.",
            _options.QueueName);

        try
        {
            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Consumidor RabbitMQ sendo finalizado.");
        }
    }

    private async Task HandleMessageAsync(
        object sender,
        BasicDeliverEventArgs eventArgs)
    {
        var consumer =
            (AsyncEventingBasicConsumer)sender;

        var channel = consumer.Channel;

        try
        {
            var message =
                JsonSerializer.Deserialize<
                    FiscalDocumentReceivedMessage>(
                        eventArgs.Body.Span,
                        SerializerOptions);

            if (message is null)
            {
                throw new JsonException(
                    "A mensagem recebida está vazia.");
            }

            _logger.LogInformation(
                """
                Documento fiscal recebido pelo consumidor.
                DocumentId: {DocumentId}
                TenantId: {TenantId}
                ExternalDocumentId: {ExternalDocumentId}
                CorrelationId: {CorrelationId}
                """,
                message.DocumentId,
                message.TenantId,
                message.ExternalDocumentId,
                message.CorrelationId);

            await channel.BasicAckAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken:
                    eventArgs.CancellationToken);
        }
        catch (OperationCanceledException)
            when (eventArgs.CancellationToken
                .IsCancellationRequested)
        {
            // Encerramento normal da aplicação.
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                """
                Falha ao processar mensagem RabbitMQ.
                DeliveryTag: {DeliveryTag}
                """,
                eventArgs.DeliveryTag);

            await channel.BasicNackAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken:
                    eventArgs.CancellationToken);
        }
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (_channel is not null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }
    }
}