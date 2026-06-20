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
}