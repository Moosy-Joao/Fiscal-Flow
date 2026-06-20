namespace FiscalFlow.Application.Documents;

public sealed record SubmitFiscalDocumentResult(string DocumentId, bool IsDuplicate);
