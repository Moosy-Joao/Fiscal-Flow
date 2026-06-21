namespace FiscalFlow.Application.Documents;

public sealed record DetectTimedOutProcessingCommand(
    TimeSpan ProcessingTimeout,
    int BatchSize,
    DateTimeOffset UtcNow);
