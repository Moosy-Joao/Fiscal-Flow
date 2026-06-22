namespace FiscalFlow.Api.Tenancy;

public sealed class TenantContext
{
    private const int MaximumLength = 100;

    public string TenantId { get; private set; } =
        string.Empty;

    public void SetTenantId(string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            tenantId);

        var normalized = tenantId.Trim();

        if (normalized.Length > MaximumLength)
        {
            throw new ArgumentException(
                $"O tenant deve possuir no máximo {MaximumLength} caracteres.",
                nameof(tenantId));
        }

        if (normalized.Any(character =>
                !char.IsLetterOrDigit(character)
                && character is not '-' and not '_' and not '.'))
        {
            throw new ArgumentException(
                "O tenant contém caracteres inválidos.",
                nameof(tenantId));
        }

        TenantId = normalized;
    }
}
