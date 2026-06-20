namespace FiscalFlow.Application.Documents;

public sealed record FailedDocumentBatchCommand(
    int MaximumAttempts,
    int BatchSize);
