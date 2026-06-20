namespace FiscalFlow.Infrastructure.Processing;

public sealed class FiscalDocumentProcessingJob
{
    private readonly IFiscalDocumentProcessor _processor;

    public FiscalDocumentProcessingJob(IFiscalDocumentProcessor processor)
    {
        _processor = processor;
    }

    public async Task ProcessAsync(string tenantId, string documentId)
    {
        await _processor.ProcessAsync(tenantId, documentId, CancellationToken.None);
    }
}
