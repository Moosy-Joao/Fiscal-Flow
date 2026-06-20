namespace FiscalFlow.Application.Documents;

public sealed record CreateFiscalDocumentResult(
    Guid Id,
    string TenantId,
    string ExternalDocumentId,
    string Status,
    DateTimeOffset ReceivedAtUtc,
    bool WasCreated);