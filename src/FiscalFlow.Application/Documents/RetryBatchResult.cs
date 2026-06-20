namespace FiscalFlow.Application.Documents;

public sealed record RetryBatchResult(
    int ClaimedCount,
    int ProcessedCount,
    int FailedCount);
