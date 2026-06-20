namespace FiscalFlow.Application.Documents;

public sealed record FiscalDocumentDataDetails(
    string AccessKey,
    string IssuerDocument,
    string IssuerName,
    string RecipientDocument,
    string RecipientName,
    decimal TotalValue,
    DateTimeOffset IssuedAt);
