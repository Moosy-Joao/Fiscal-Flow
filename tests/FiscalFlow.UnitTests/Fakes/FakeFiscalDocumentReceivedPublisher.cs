using FiscalFlow.Application.Messaging;

namespace FiscalFlow.UnitTests.Fakes;

public sealed class FakeFiscalDocumentReceivedPublisher :
    IFiscalDocumentReceivedPublisher
{
    public List<FiscalDocumentReceivedMessage> Messages
    {
        get;
    } = [];

    public Task PublishAsync(
        FiscalDocumentReceivedMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        Messages.Add(message);

        return Task.CompletedTask;
    }
}