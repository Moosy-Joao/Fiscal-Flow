using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

public interface IFiscalDocumentRepository
{
    Task InsertAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default);

    Task<FiscalDocumentDetails?> FindByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<FiscalDocument?> FindDomainByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default);

    Task<PagedResult<FiscalDocumentDetails>> ListAsync(
    ListFiscalDocumentsQuery query,
    CancellationToken cancellationToken = default);
}