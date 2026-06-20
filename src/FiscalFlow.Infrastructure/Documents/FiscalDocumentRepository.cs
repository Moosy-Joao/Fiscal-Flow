using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
using FiscalFlow.Infrastructure.MongoDb;
using MongoDB.Driver;

namespace FiscalFlow.Infrastructure.Documents;

public sealed class FiscalDocumentRepository
    : IFiscalDocumentRepository
{
    private const string CollectionName =
        "fiscalDocuments";

    private readonly IMongoCollection<FiscalDocumentMongoModel>
        _collection;

    public FiscalDocumentRepository(
        MongoDbContext mongoDbContext)
    {
        _collection =
            mongoDbContext.GetCollection<FiscalDocumentMongoModel>(
                CollectionName);
    }

    public async Task InsertAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        var mongoModel = new FiscalDocumentMongoModel
        {
            Id = document.Id.ToString(),
            TenantId = document.TenantId,
            ExternalDocumentId =
                document.ExternalDocumentId,
            Status = document.Status.ToString(),
            ReceivedAtUtc =
                document.ReceivedAtUtc.UtcDateTime,
            ProcessedAtUtc =
                document.ProcessedAtUtc?.UtcDateTime,
            FailureReason = document.FailureReason
        };

        await _collection.InsertOneAsync(
            mongoModel,
            cancellationToken: cancellationToken);
    }
}