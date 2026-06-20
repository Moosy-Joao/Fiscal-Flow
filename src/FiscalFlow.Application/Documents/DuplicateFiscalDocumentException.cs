namespace FiscalFlow.Application.Documents;

public sealed class DuplicateFiscalDocumentException
    : Exception
{
    public DuplicateFiscalDocumentException(
        string tenantId,
        string externalDocumentId,
        Exception? innerException = null)
        : base(
            $"Já existe o documento fiscal " +
            $"'{externalDocumentId}' para o tenant " +
            $"'{tenantId}'.",
            innerException)
    {
        TenantId = tenantId;
        ExternalDocumentId = externalDocumentId;
    }

    public string TenantId { get; }

    public string ExternalDocumentId { get; }
}