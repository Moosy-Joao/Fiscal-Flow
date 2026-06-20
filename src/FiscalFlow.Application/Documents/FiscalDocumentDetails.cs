namespace FiscalFlow.Application.Documents;

public sealed record FiscalDocumentDetails(
    Guid Id,
    string TenantId,
    string ExternalDocumentId,
    string Status,
    DateTimeOffset ReceivedAtUtc,
    DateTimeOffset? ProcessedAtUtc,
    string? FailureReason);