namespace FiscalFlow.Application.Documents;

public interface IDocumentCleanupRepository
{
    Task<int> DeleteOldFinalDocumentsAsync(
        DateTimeOffset receivedBeforeUtc,
        int batchSize,
        CancellationToken cancellationToken = default);
}
