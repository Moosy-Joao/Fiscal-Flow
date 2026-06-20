namespace FiscalFlow.Domain.Documents;

public sealed record FiscalDocumentData(
    string AccessKey,
    string IssuerDocument,
    string IssuerName,
    string RecipientDocument,
    string RecipientName,
    decimal TotalValue,
    DateTimeOffset IssuedAt);
