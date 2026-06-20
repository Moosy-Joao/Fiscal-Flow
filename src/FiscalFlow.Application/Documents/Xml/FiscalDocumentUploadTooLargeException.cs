namespace FiscalFlow.Application.Documents.Xml;

public sealed class FiscalDocumentUploadTooLargeException :
    FiscalDocumentUploadValidationException
{
    public FiscalDocumentUploadTooLargeException(
        long maximumSizeBytes)
        : base(
            $"O arquivo XML excede o limite de {maximumSizeBytes} bytes.")
    {
        MaximumSizeBytes = maximumSizeBytes;
    }

    public long MaximumSizeBytes { get; }
}
