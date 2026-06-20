using FiscalFlow.Application.Documents.Xml;
using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

internal static class FiscalDocumentDataMapper
{
    public static FiscalDocumentData Map(
        FiscalDocumentXmlData value)
    {
        return new FiscalDocumentData(
            value.AccessKey,
            value.IssuerDocument,
            value.IssuerName,
            value.RecipientDocument,
            value.RecipientName,
            value.TotalValue,
            value.IssuedAt);
    }
}
