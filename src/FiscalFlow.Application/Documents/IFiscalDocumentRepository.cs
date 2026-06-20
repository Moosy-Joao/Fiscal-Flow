using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

public interface IFiscalDocumentRepository
{
    Task<FiscalDocument?> GetByIdAsync(string tenantId, string documentId, CancellationToken cancellationToken);
    Task InsertAsync(FiscalDocument document, CancellationToken cancellationToken);
    Task UpdateStatusAsync(string tenantId, string documentId, DocumentProcessingStatus status, DateTimeOffset? processedAtUtc, CancellationToken cancellationToken);
}
