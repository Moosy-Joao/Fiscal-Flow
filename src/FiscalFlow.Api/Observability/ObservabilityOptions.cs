namespace FiscalFlow.Api.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public string ServiceName { get; init; } = "FiscalFlow";

    public string ServiceVersion { get; init; } = "1.0.0";

    public bool OtlpEnabled { get; init; }

    public string? OtlpEndpoint { get; init; }
}
