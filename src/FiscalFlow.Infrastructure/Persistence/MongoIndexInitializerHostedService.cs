using FiscalFlow.Domain.Documents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FiscalFlow.Infrastructure.Persistence;

public sealed class MongoIndexInitializerHostedService : IHostedService
{
    private readonly IMongoClient _mongoClient;
    private readonly MongoOptions _options;

    public MongoIndexInitializerHostedService(IMongoClient mongoClient, IOptions<MongoOptions> options)
    {
        _mongoClient = mongoClient;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var database = _mongoClient.GetDatabase(_options.DatabaseName);

        var fiscalDocuments = database.GetCollection<FiscalDocument>("fiscal_documents");
        var fiscalDocumentIndexKeys = Builders<FiscalDocument>.IndexKeys
            .Ascending(x => x.TenantId)
            .Ascending(x => x.ExternalDocumentId);
        await fiscalDocuments.Indexes.CreateOneAsync(
            new CreateIndexModel<FiscalDocument>(fiscalDocumentIndexKeys),
            cancellationToken: cancellationToken);

        var idempotencyRecords = database.GetCollection<IdempotencyRecord>("idempotency_records");
        var idempotencyIndexKeys = Builders<IdempotencyRecord>.IndexKeys
            .Ascending(x => x.TenantId)
            .Ascending(x => x.IdempotencyKey);
        await idempotencyRecords.Indexes.CreateOneAsync(
            new CreateIndexModel<IdempotencyRecord>(idempotencyIndexKeys, new CreateIndexOptions { Unique = true }),
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
