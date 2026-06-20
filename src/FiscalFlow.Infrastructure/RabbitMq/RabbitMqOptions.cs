namespace FiscalFlow.Infrastructure.RabbitMq;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } =
        string.Empty;

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } =
        string.Empty;

    public string Password { get; init; } =
        string.Empty;

    public string VirtualHost { get; init; } = "/";

    public string ExchangeName { get; init; } =
        "fiscalflow.documents";

    public string QueueName { get; init; } =
        "fiscalflow.documents.process";

    public string RoutingKey { get; init; } =
        "fiscal-document.received";

    public string RetryExchangeName { get; init; } =
        "fiscalflow.documents.retry";

    public string RetryQueueName { get; init; } =
        "fiscalflow.documents.process.retry";

    public string RetryRoutingKey { get; init; } =
        "fiscal-document.received.retry";

    public string DeadLetterExchangeName { get; init; } =
        "fiscalflow.documents.dead-letter";

    public string DeadLetterQueueName { get; init; } =
        "fiscalflow.documents.process.dead-letter";

    public string DeadLetterRoutingKey { get; init; } =
        "fiscal-document.received.dead-letter";

    public int RetryDelayMilliseconds { get; init; } =
        5000;

    public int MaxRetryAttempts { get; init; } = 3;
}