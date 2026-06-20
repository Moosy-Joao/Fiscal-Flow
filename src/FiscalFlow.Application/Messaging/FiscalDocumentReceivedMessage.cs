namespace FiscalFlow.Application.Messaging;

public sealed record FiscalDocumentReceivedMessage(
    Guid DocumentId,
    string TenantId,
    string ExternalDocumentId,
    DateTimeOffset ReceivedAtUtc,
    Guid CorrelationId);