namespace FiscalFlow.Application.Documents;

public sealed record CreateFiscalDocumentCommand(
    string TenantId,
    string ExternalDocumentId,
    string XmlContent);