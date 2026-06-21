namespace FiscalFlow.Api.Configuration;

public sealed class BackgroundJobsOptions
{
    public const string SectionName = "BackgroundJobs";

    public bool Enabled { get; init; }

    public string DatabaseName { get; init; } =
        "fiscalflow-jobs";

    public string CollectionPrefix { get; init; } =
        "hangfire";

    public int WorkerCount { get; init; } = 1;

    public string FailedRetryCron { get; init; } =
        "*/5 * * * *";

    public int FailedBatchSize { get; init; } = 20;

    public int MaximumFailedAttempts { get; init; } = 3;

    public string TimedOutProcessingCron { get; init; } =
        "*/5 * * * *";

    public int TimedOutProcessingBatchSize { get; init; } = 20;

    public int ProcessingTimeoutMinutes { get; init; } = 15;

    public string CleanupCron { get; init; } =
        "0 3 * * *";

    public int DocumentRetentionDays { get; init; } = 90;

    public int CleanupBatchSize { get; init; } = 100;
}
