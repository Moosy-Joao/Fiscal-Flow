namespace FiscalFlow.Api.Middleware;

public sealed class TenantContext
{
    public const string HeaderName = "X-Tenant-Id";

    public string TenantId { get; set; } = string.Empty;
}
