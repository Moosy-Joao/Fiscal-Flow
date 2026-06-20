namespace FiscalFlow.Application.Documents;

public sealed record ProcessFiscalDocumentCommand(
    Guid DocumentId,
    string TenantId);