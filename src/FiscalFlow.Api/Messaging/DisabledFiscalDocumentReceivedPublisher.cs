using FiscalFlow.Application.Messaging;

namespace FiscalFlow.Api.Messaging;

internal sealed class DisabledFiscalDocumentReceivedPublisher
    : IFiscalDocumentReceivedPublisher
{
    private readonly ILogger<
        DisabledFiscalDocumentReceivedPublisher> _logger;

    public DisabledFiscalDocumentReceivedPublisher(
        ILogger<DisabledFiscalDocumentReceivedPublisher> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public Task PublishAsync(
        FiscalDocumentReceivedMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        _logger.LogWarning(
            "RabbitMQ desabilitado. A mensagem do documento {DocumentId} não foi publicada.",
            message.DocumentId);

        return Task.CompletedTask;
    }
}
