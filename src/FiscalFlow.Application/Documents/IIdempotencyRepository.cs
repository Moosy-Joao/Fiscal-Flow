using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

public interface IIdempotencyRepository
{
    Task<IdempotencyRecord?> GetAsync(string tenantId, string idempotencyKey, CancellationToken cancellationToken);
    Task<bool> TryInsertAsync(IdempotencyRecord record, CancellationToken cancellationToken);
}
