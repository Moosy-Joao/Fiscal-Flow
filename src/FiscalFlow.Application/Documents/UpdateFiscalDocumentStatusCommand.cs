using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

public sealed record UpdateFiscalDocumentStatusCommand(
    Guid Id,
    DocumentProcessingStatus Status,
    string? FailureReason);