namespace FiscalFlow.Api.Security;

public sealed class ApiSecurityOptions
{
    public const string SectionName = "Security";

    public bool Enabled { get; init; }

    public string Issuer { get; init; } = "FiscalFlow";

    public string Audience { get; init; } = "FiscalFlow.Api";

    public string? SigningKey { get; init; }

    public int ClockSkewSeconds { get; init; } = 60;

    public int RateLimitPermitLimit { get; init; } = 60;

    public int RateLimitWindowSeconds { get; init; } = 60;
}
