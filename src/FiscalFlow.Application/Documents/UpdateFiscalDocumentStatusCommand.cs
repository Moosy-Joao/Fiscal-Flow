using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

public sealed record UpdateFiscalDocumentStatusCommand(
    Guid Id,
    string TenantId,
    DocumentProcessingStatus Status,
    string? FailureReason);