namespace FiscalFlow.Infrastructure.Processing;

public interface IFiscalDocumentProcessor
{
    Task ProcessAsync(string tenantId, string documentId, CancellationToken cancellationToken);
}
