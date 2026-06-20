namespace FiscalFlow.Api.Tenancy;

public sealed class TenantContext
{
    public string TenantId { get; private set; } =
        string.Empty;

    public void SetTenantId(string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            tenantId);

        TenantId = tenantId.Trim();
    }
}