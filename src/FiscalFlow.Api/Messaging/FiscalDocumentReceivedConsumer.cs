using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FiscalFlow.Application.Documents;
using FiscalFlow.Application.Messaging;
using FiscalFlow.Application.Observability;
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

    private readonly IServiceScopeFactory
        _scopeFactory;

    private readonly ILogger<
        FiscalDocumentReceivedConsumer> _logger;

    private IChannel? _channel;

    public FiscalDocumentReceivedConsumer(
        RabbitMqConnectionFactory connectionFactory,
        RabbitMqOptions options,
        IServiceScopeFactory scopeFactory,
        ILogger<FiscalDocumentReceivedConsumer> logger)
    {
        ArgumentNullException.ThrowIfNull(
            connectionFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionFactory = connectionFactory;
        _options = options;
        _scopeFactory = scopeFactory;
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
        var traceContext = RabbitMqTraceContext.Extract(
            eventArgs.BasicProperties);

        using var activity = traceContext.HasParent
            ? FiscalFlowTelemetry.ActivitySource.StartActivity(
                "rabbitmq consume fiscal-document.received",
                ActivityKind.Consumer,
                traceContext.ParentContext)
            : FiscalFlowTelemetry.ActivitySource.StartActivity(
                "rabbitmq consume fiscal-document.received",
                ActivityKind.Consumer);

        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag(
            "messaging.destination.name",
            _options.QueueName);
        activity?.SetTag(
            "messaging.operation.type",
            "process");
        activity?.SetTag(
            "messaging.message.id",
            eventArgs.BasicProperties.MessageId);

        var correlationId =
            traceContext.CorrelationId
            ?? eventArgs.BasicProperties.MessageId
            ?? Guid.NewGuid().ToString("N");

        activity?.SetTag(
            "correlation.id",
            correlationId);

        using var logScope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["MessageId"] =
                    eventArgs.BasicProperties.MessageId
                    ?? string.Empty
            });

        var startedAt = Stopwatch.GetTimestamp();

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

            activity?.SetTag(
                "fiscal.document.id",
                message.DocumentId);

            using var documentScope = _logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["DocumentId"] = message.DocumentId,
                    ["TenantId"] = message.TenantId,
                    ["ExternalDocumentId"] =
                        message.ExternalDocumentId
                });

            FiscalFlowTelemetry.DocumentsReceived.Add(1);

            _logger.LogInformation(
                "Documento fiscal recebido pelo consumidor.");

            using var scope =
                _scopeFactory.CreateScope();

            var processingService =
                scope.ServiceProvider
                    .GetRequiredService<
                        ProcessFiscalDocumentService>();

            var command =
                new ProcessFiscalDocumentCommand(
                    message.DocumentId,
                    message.TenantId);

            await processingService.ExecuteAsync(
                command,
                eventArgs.CancellationToken);

            FiscalFlowTelemetry.DocumentsProcessed.Add(1);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Documento fiscal processado com sucesso.");

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
            activity?.SetStatus(
                ActivityStatusCode.Error,
                "Processamento cancelado.");
        }
        catch (Exception exception)
        {
            FiscalFlowTelemetry.DocumentsFailed.Add(1);

            activity?.SetStatus(
                ActivityStatusCode.Error,
                exception.Message);

            activity?.SetTag(
                "error.type",
                exception.GetType().FullName);

            await HandleProcessingFailureAsync(
                channel,
                eventArgs,
                exception);
        }
        finally
        {
            FiscalFlowTelemetry.ProcessingDuration.Record(
                Stopwatch.GetElapsedTime(startedAt)
                    .TotalMilliseconds);
        }
    }

    private async Task HandleProcessingFailureAsync(
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        Exception exception)
    {
        var retryCount =
            GetRetryCount(
                eventArgs.BasicProperties.Headers);

        if (retryCount >=
            _options.MaxRetryAttempts)
        {
            await PublishToDeadLetterQueueAsync(
                channel,
                eventArgs,
                exception,
                retryCount);

            FiscalFlowTelemetry.DocumentsDeadLettered.Add(1);

            await channel.BasicAckAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken:
                    eventArgs.CancellationToken);

            _logger.LogError(
                exception,
                "Mensagem enviada para a Dead Letter Queue. RetryCount: {RetryCount}. QueueName: {QueueName}.",
                retryCount,
                _options.DeadLetterQueueName);

            return;
        }

        var nextRetryAttempt =
            retryCount + 1;

        _logger.LogWarning(
            exception,
            "Falha ao processar mensagem. RetryAttempt: {RetryAttempt}. MaxRetryAttempts: {MaxRetryAttempts}.",
            nextRetryAttempt,
            _options.MaxRetryAttempts);

        await channel.BasicNackAsync(
            deliveryTag: eventArgs.DeliveryTag,
            multiple: false,
            requeue: false,
            cancellationToken:
                eventArgs.CancellationToken);
    }

    private async Task PublishToDeadLetterQueueAsync(
        IChannel channel,
        BasicDeliverEventArgs eventArgs,
        Exception exception,
        long retryCount)
    {
        var properties =
            new BasicProperties(
                eventArgs.BasicProperties);

        properties.Headers =
            eventArgs.BasicProperties.Headers is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(
                    eventArgs.BasicProperties.Headers);

        properties.Headers["x-retry-count"] =
            retryCount;

        properties.Headers["x-processing-attempt-count"] =
            retryCount + 1;

        properties.Headers["x-final-error-type"] =
            exception.GetType().FullName
            ?? exception.GetType().Name;

        properties.Headers["x-final-error-message"] =
            exception.Message;

        await channel.BasicPublishAsync(
            exchange:
                _options.DeadLetterExchangeName,
            routingKey:
                _options.DeadLetterRoutingKey,
            mandatory: true,
            basicProperties: properties,
            body: eventArgs.Body,
            cancellationToken:
                eventArgs.CancellationToken);
    }

    private long GetRetryCount(
        IDictionary<string, object?>? headers)
    {
        if (headers is null
            || !headers.TryGetValue(
                "x-death",
                out var rawDeaths)
            || rawDeaths is not
                IEnumerable<object?> deaths)
        {
            return 0;
        }

        foreach (var rawDeath in deaths)
        {
            if (rawDeath is not
                IDictionary<string, object?> death)
            {
                continue;
            }

            var queue =
                ReadAmqpString(
                    GetValue(death, "queue"));

            var reason =
                ReadAmqpString(
                    GetValue(death, "reason"));

            if (!string.Equals(
                    queue,
                    _options.QueueName,
                    StringComparison.Ordinal)
                || !string.Equals(
                    reason,
                    "rejected",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return ReadAmqpCount(
                GetValue(death, "count"));
        }

        return 0;
    }

    private static object? GetValue(
        IDictionary<string, object?> values,
        string key)
    {
        return values.TryGetValue(
            key,
            out var value)
                ? value
                : null;
    }

    private static string? ReadAmqpString(
        object? value)
    {
        return value switch
        {
            string text => text,
            byte[] bytes =>
                Encoding.UTF8.GetString(bytes),
            ReadOnlyMemory<byte> memory =>
                Encoding.UTF8.GetString(
                    memory.Span),
            ArraySegment<byte> segment =>
                Encoding.UTF8.GetString(
                    segment.Array!,
                    segment.Offset,
                    segment.Count),
            _ => null
        };
    }

    private static long ReadAmqpCount(
        object? value)
    {
        return value switch
        {
            byte number => number,
            sbyte number => number,
            short number => number,
            ushort number => number,
            int number => number,
            uint number => number,
            long number => number,
            ulong number
                when number <= long.MaxValue =>
                    (long)number,
            _ => 0
        };
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
