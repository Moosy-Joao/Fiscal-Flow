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
}
