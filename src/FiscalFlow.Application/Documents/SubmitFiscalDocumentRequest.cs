using System.Text.Json;

namespace FiscalFlow.Application.Documents;

public sealed class SubmitFiscalDocumentRequest
{
    public required string ExternalDocumentId { get; init; }
    public required JsonElement Payload { get; init; }
}
