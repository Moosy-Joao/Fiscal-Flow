using FiscalFlow.Application.Messaging;

namespace FiscalFlow.UnitTests.Fakes;

public sealed class FakeFiscalDocumentReceivedPublisher :
    IFiscalDocumentReceivedPublisher
{
    public List<FiscalDocumentReceivedMessage> Messages
    {
        get;
    } = [];

    public int PublishAttempts { get; private set; }

    public Exception? ExceptionToThrow { get; set; }

    public Task PublishAsync(
        FiscalDocumentReceivedMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        PublishAttempts++;

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        Messages.Add(message);

        return Task.CompletedTask;
    }
}
