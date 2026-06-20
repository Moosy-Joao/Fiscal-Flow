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

        var mongoModel = MapToMongoModel(document);

        await _collection.InsertOneAsync(
            mongoModel,
            cancellationToken: cancellationToken);
    }

    public async Task<FiscalDocumentDetails?> FindByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var mongoModel = await FindMongoModelByIdAsync(
            id,
            cancellationToken);

        if (mongoModel is null)
        {
            return null;
        }

        return MapToDetails(mongoModel);
    }

    public async Task<FiscalDocument?> FindDomainByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var mongoModel = await FindMongoModelByIdAsync(
            id,
            cancellationToken);

        if (mongoModel is null)
        {
            return null;
        }

        if (!Enum.TryParse<DocumentProcessingStatus>(
                mongoModel.Status,
                ignoreCase: true,
                out var status))
        {
            throw new InvalidOperationException(
                $"O status '{mongoModel.Status}' armazenado no MongoDB é inválido.");
        }

        return FiscalDocument.Rehydrate(
            Guid.Parse(mongoModel.Id),
            mongoModel.TenantId,
            mongoModel.ExternalDocumentId,
            status,
            ToDateTimeOffset(mongoModel.ReceivedAtUtc),
            mongoModel.ProcessedAtUtc is null
                ? null
                : ToDateTimeOffset(
                    mongoModel.ProcessedAtUtc.Value),
            mongoModel.FailureReason);
    }

    public async Task UpdateAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        var mongoModel = MapToMongoModel(document);

        var result = await _collection.ReplaceOneAsync(
            storedDocument =>
                storedDocument.Id == mongoModel.Id,
            mongoModel,
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException(
                $"O documento {document.Id} não foi encontrado durante a atualização.");
        }
    }

    private async Task<FiscalDocumentMongoModel?>
        FindMongoModelByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        var idAsString = id.ToString();

        return await _collection
            .Find(document => document.Id == idAsString)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static FiscalDocumentMongoModel MapToMongoModel(
        FiscalDocument document)
    {
        return new FiscalDocumentMongoModel
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
    }

    private static FiscalDocumentDetails MapToDetails(
        FiscalDocumentMongoModel mongoModel)
    {
        return new FiscalDocumentDetails(
            Guid.Parse(mongoModel.Id),
            mongoModel.TenantId,
            mongoModel.ExternalDocumentId,
            mongoModel.Status,
            ToDateTimeOffset(mongoModel.ReceivedAtUtc),
            mongoModel.ProcessedAtUtc is null
                ? null
                : ToDateTimeOffset(
                    mongoModel.ProcessedAtUtc.Value),
            mongoModel.FailureReason);
    }

    private static DateTimeOffset ToDateTimeOffset(
        DateTime dateTime)
    {
        var utcDateTime = DateTime.SpecifyKind(
            dateTime,
            DateTimeKind.Utc);

        return new DateTimeOffset(utcDateTime);
    }
}