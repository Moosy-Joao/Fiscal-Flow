namespace FiscalFlow.Application.Messaging;

public interface IFiscalDocumentReceivedPublisher
{
    Task PublishAsync(
        FiscalDocumentReceivedMessage message,
        CancellationToken cancellationToken = default);
}