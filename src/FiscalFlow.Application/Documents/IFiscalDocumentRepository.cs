using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

public interface IFiscalDocumentRepository
{
    Task InsertAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default);

    Task<FiscalDocumentDetails?> FindByIdAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<FiscalDocumentDetails?> FindByExternalDocumentIdAsync(
        string tenantId,
        string externalDocumentId,
        CancellationToken cancellationToken = default);

    Task<FiscalDocument?> FindDomainByIdAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<FiscalDocument?> TryStartProcessingAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FiscalDocument>>
        ClaimFailedForReprocessingAsync(
            int maximumAttempts,
            int batchSize,
            CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<FiscalDocument> documents =
            Array.Empty<FiscalDocument>();

        return Task.FromResult(documents);
    }

    Task UpdateAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default);

    Task<PagedResult<FiscalDocumentDetails>> ListAsync(
        ListFiscalDocumentsQuery query,
        CancellationToken cancellationToken = default);
}
