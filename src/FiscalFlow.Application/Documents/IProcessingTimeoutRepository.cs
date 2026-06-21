namespace FiscalFlow.Application.Documents;

public interface IProcessingTimeoutRepository
{
    Task<int> MarkTimedOutProcessingAsFailedAsync(
        DateTimeOffset startedBeforeUtc,
        int batchSize,
        CancellationToken cancellationToken = default);
}
