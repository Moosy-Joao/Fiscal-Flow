namespace FiscalFlow.Application.Documents;

public interface IDocumentDispatchQueue
{
    Task PublishAsync(string tenantId, string documentId, CancellationToken cancellationToken);
}
