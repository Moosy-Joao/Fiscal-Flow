namespace FiscalFlow.Application.Documents.Xml;

public sealed record FiscalDocumentXmlData(
    string AccessKey,
    string IssuerDocument,
    string IssuerName,
    string RecipientDocument,
    string RecipientName,
    decimal TotalValue,
    DateTimeOffset IssuedAt);