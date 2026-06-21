using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
using FiscalFlow.Infrastructure.MongoDb;
using MongoDB.Driver;

namespace FiscalFlow.Infrastructure.Documents;

public sealed class DocumentCleanupRepository
    : IDocumentCleanupRepository
{
    private const string CollectionName = "fiscalDocuments";

    private readonly IMongoCollection<FiscalDocumentMongoModel>
        _collection;

    public DocumentCleanupRepository(MongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext
            .GetCollection<FiscalDocumentMongoModel>(CollectionName);
    }

    public async Task<int> DeleteOldFinalDocumentsAsync(
        DateTimeOffset receivedBeforeUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize),
                "O tamanho do lote deve ser maior que zero.");
        }

        var filterBuilder =
            Builders<FiscalDocumentMongoModel>.Filter;

        var finalStatuses = new[]
        {
            DocumentProcessingStatus.Processed.ToString(),
            DocumentProcessingStatus.Failed.ToString()
        };

        var cutoff = receivedBeforeUtc.UtcDateTime;

        var eligibleFilter = filterBuilder.And(
            filterBuilder.In(
                document => document.Status,
                finalStatuses),
            filterBuilder.Lt(
                document => document.ReceivedAtUtc,
                cutoff));

        var documentIds = await _collection
            .Find(eligibleFilter)
            .SortBy(document => document.ReceivedAtUtc)
            .Limit(batchSize)
            .Project(document => document.Id)
            .ToListAsync(cancellationToken);

        if (documentIds.Count == 0)
        {
            return 0;
        }

        var deleteFilter = filterBuilder.And(
            filterBuilder.In(
                document => document.Id,
                documentIds),
            eligibleFilter);

        var result = await _collection.DeleteManyAsync(
            deleteFilter,
            cancellationToken);

        return checked((int)result.DeletedCount);
    }
}
