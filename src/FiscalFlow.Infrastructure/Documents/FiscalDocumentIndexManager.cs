using FiscalFlow.Infrastructure.MongoDb;
using MongoDB.Driver;

namespace FiscalFlow.Infrastructure.Documents;

public sealed class FiscalDocumentIndexManager
{
    private const string CollectionName =
        "fiscalDocuments";

    private readonly IMongoCollection<FiscalDocumentMongoModel>
        _collection;

    public FiscalDocumentIndexManager(
        MongoDbContext mongoDbContext)
    {
        _collection =
            mongoDbContext.GetCollection<FiscalDocumentMongoModel>(
                CollectionName);
    }

    public async Task EnsureCreatedAsync(
        CancellationToken cancellationToken = default)
    {
        var indexKeys =
            Builders<FiscalDocumentMongoModel>.IndexKeys;

        var uniqueDocumentIndex =
            new CreateIndexModel<FiscalDocumentMongoModel>(
                indexKeys
                    .Ascending(document => document.TenantId)
                    .Ascending(
                        document =>
                            document.ExternalDocumentId),
                new CreateIndexOptions
                {
                    Name =
                        "ux_fiscal_documents_tenant_external_id",
                    Unique = true
                });

        var tenantAndDateIndex =
            new CreateIndexModel<FiscalDocumentMongoModel>(
                indexKeys
                    .Ascending(document => document.TenantId)
                    .Descending(
                        document =>
                            document.ReceivedAtUtc),
                new CreateIndexOptions
                {
                    Name =
                        "ix_fiscal_documents_tenant_received_at"
                });

        var tenantStatusAndDateIndex =
            new CreateIndexModel<FiscalDocumentMongoModel>(
                indexKeys
                    .Ascending(document => document.TenantId)
                    .Ascending(document => document.Status)
                    .Descending(
                        document =>
                            document.ReceivedAtUtc),
                new CreateIndexOptions
                {
                    Name =
                        "ix_fiscal_documents_tenant_status_received_at"
                });

        var processingTimeoutIndex =
            new CreateIndexModel<FiscalDocumentMongoModel>(
                indexKeys
                    .Ascending(document => document.Status)
                    .Ascending(
                        document =>
                            document.ProcessingStartedAtUtc)
                    .Ascending(
                        document =>
                            document.ReceivedAtUtc),
                new CreateIndexOptions
                {
                    Name =
                        "ix_fiscal_documents_status_processing_started_at"
                });

        var indexes = new[]
        {
            uniqueDocumentIndex,
            tenantAndDateIndex,
            tenantStatusAndDateIndex,
            processingTimeoutIndex
        };

        await _collection.Indexes.CreateManyAsync(
            indexes,
            cancellationToken: cancellationToken);
    }
}
