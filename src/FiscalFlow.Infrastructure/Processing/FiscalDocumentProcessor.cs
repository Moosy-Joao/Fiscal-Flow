using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Infrastructure.Processing;

public sealed class FiscalDocumentProcessor : IFiscalDocumentProcessor
{
    private readonly IFiscalDocumentRepository _documents;

    public FiscalDocumentProcessor(IFiscalDocumentRepository documents)
    {
        _documents = documents;
    }

    public async Task ProcessAsync(string tenantId, string documentId, CancellationToken cancellationToken)
    {
        await _documents.UpdateStatusAsync(tenantId, documentId, DocumentProcessingStatus.Processing, null, cancellationToken);

        await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);

        await _documents.UpdateStatusAsync(tenantId, documentId, DocumentProcessingStatus.Processed, DateTimeOffset.UtcNow, cancellationToken);
    }
}
