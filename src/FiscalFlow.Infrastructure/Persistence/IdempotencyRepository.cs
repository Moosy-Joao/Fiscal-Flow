using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FiscalFlow.Infrastructure.Persistence;

public sealed class IdempotencyRepository : IIdempotencyRepository
{
    private readonly IMongoCollection<IdempotencyRecord> _collection;

    public IdempotencyRepository(IMongoClient mongoClient, IOptions<MongoOptions> options)
    {
        var database = mongoClient.GetDatabase(options.Value.DatabaseName);
        _collection = database.GetCollection<IdempotencyRecord>("idempotency_records");
    }

    public async Task<IdempotencyRecord?> GetAsync(string tenantId, string idempotencyKey, CancellationToken cancellationToken)
    {
        return await _collection
            .Find(x => x.TenantId == tenantId && x.IdempotencyKey == idempotencyKey)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> TryInsertAsync(IdempotencyRecord record, CancellationToken cancellationToken)
    {
        try
        {
            await _collection.InsertOneAsync(record, cancellationToken: cancellationToken);
            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }
}
