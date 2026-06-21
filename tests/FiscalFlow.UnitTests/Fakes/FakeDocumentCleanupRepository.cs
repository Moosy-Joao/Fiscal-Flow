using FiscalFlow.Application.Documents;

namespace FiscalFlow.UnitTests.Fakes;

internal sealed class FakeDocumentCleanupRepository
    : IDocumentCleanupRepository
{
    public DateTimeOffset? ReceivedBeforeUtc { get; private set; }

    public int? BatchSize { get; private set; }

    public int Result { get; set; }

    public Task<int> DeleteOldFinalDocumentsAsync(
        DateTimeOffset receivedBeforeUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        ReceivedBeforeUtc = receivedBeforeUtc;
        BatchSize = batchSize;

        return Task.FromResult(Result);
    }
}
