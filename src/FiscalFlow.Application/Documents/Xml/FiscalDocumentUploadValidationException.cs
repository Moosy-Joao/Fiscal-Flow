namespace FiscalFlow.Application.Documents.Xml;

public class FiscalDocumentUploadValidationException :
    Exception
{
    public FiscalDocumentUploadValidationException(
        string message)
        : base(message)
    {
    }

    public FiscalDocumentUploadValidationException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
