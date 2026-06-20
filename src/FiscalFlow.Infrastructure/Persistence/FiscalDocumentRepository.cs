using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FiscalFlow.Infrastructure.Persistence;

public sealed class FiscalDocumentRepository : IFiscalDocumentRepository
{
    private readonly IMongoCollection<FiscalDocument> _collection;

    public FiscalDocumentRepository(IMongoClient mongoClient, IOptions<MongoOptions> options)
    {
        var database = mongoClient.GetDatabase(options.Value.DatabaseName);
        _collection = database.GetCollection<FiscalDocument>("fiscal_documents");

        var indexKeys = Builders<FiscalDocument>.IndexKeys
            .Ascending(x => x.TenantId)
            .Ascending(x => x.ExternalDocumentId);
        _collection.Indexes.CreateOne(
            new CreateIndexModel<FiscalDocument>(indexKeys, new CreateIndexOptions { Background = true }));
    }

    public async Task<FiscalDocument?> GetByIdAsync(string tenantId, string documentId, CancellationToken cancellationToken)
    {
        return await _collection
            .Find(x => x.TenantId == tenantId && x.Id == documentId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task InsertAsync(FiscalDocument document, CancellationToken cancellationToken)
    {
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task UpdateStatusAsync(string tenantId, string documentId, DocumentProcessingStatus status, DateTimeOffset? processedAtUtc, CancellationToken cancellationToken)
    {
        var update = Builders<FiscalDocument>.Update
            .Set(x => x.Status, status)
            .Set(x => x.ProcessedAtUtc, processedAtUtc);

        await _collection.UpdateOneAsync(
            x => x.TenantId == tenantId && x.Id == documentId,
            update,
            cancellationToken: cancellationToken);
    }
}
