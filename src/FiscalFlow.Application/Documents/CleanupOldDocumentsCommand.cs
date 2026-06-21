namespace FiscalFlow.Application.Documents;

public sealed record CleanupOldDocumentsCommand(
    int RetentionDays,
    int BatchSize,
    DateTimeOffset UtcNow);
