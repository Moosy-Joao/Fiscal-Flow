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

        try
        {
            await _collection.InsertOneAsync(
                mongoModel,
                cancellationToken: cancellationToken);
        }
        catch (MongoWriteException exception)
            when (exception.WriteError.Category
                  == ServerErrorCategory.DuplicateKey)
        {
            throw new DuplicateFiscalDocumentException(
                document.TenantId,
                document.ExternalDocumentId,
                exception);
        }
    }

    public async Task<FiscalDocumentDetails?> FindByIdAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var mongoModel = await FindMongoModelByIdAsync(
            id,
            tenantId,
            cancellationToken);

        if (mongoModel is null)
        {
            return null;
        }

        return MapToDetails(mongoModel);
    }

    public async Task<FiscalDocumentDetails?>
    FindByExternalDocumentIdAsync(
        string tenantId,
        string externalDocumentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            tenantId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            externalDocumentId);

        var normalizedTenantId = tenantId.Trim();
        var normalizedExternalDocumentId =
            externalDocumentId.Trim();

        var mongoModel = await _collection
            .Find(document =>
                document.TenantId == normalizedTenantId
                && document.ExternalDocumentId
                    == normalizedExternalDocumentId)
            .FirstOrDefaultAsync(cancellationToken);

        return mongoModel is null
            ? null
            : MapToDetails(mongoModel);
    }

    public async Task<FiscalDocument?> FindDomainByIdAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var mongoModel = await FindMongoModelByIdAsync(
            id,
            tenantId,
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
            ToDateTimeOffset(
                mongoModel.ReceivedAtUtc),
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
                storedDocument.Id == mongoModel.Id
                && storedDocument.TenantId
                    == mongoModel.TenantId,
            mongoModel,
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException(
                $"O documento {document.Id} não foi encontrado durante a atualização.");
        }
    }

    public async Task<PagedResult<FiscalDocumentDetails>>
        ListAsync(
            ListFiscalDocumentsQuery query,
            CancellationToken cancellationToken = default)
    {
        var filterBuilder =
            Builders<FiscalDocumentMongoModel>.Filter;

        var filters =
            new List<FilterDefinition<FiscalDocumentMongoModel>>
            {
                filterBuilder.Eq(
                    document => document.TenantId,
                    query.TenantId)
            };

        if (query.Status is not null)
        {
            filters.Add(
                filterBuilder.Eq(
                    document => document.Status,
                    query.Status.Value.ToString()));
        }

        var filter = filterBuilder.And(filters);

        var totalItems =
            await _collection.CountDocumentsAsync(
                filter,
                cancellationToken: cancellationToken);

        var mongoModels = await _collection
            .Find(filter)
            .SortByDescending(
                document => document.ReceivedAtUtc)
            .Skip(
                (query.Page - 1)
                * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = mongoModels
            .Select(MapToDetails)
            .ToList();

        return new PagedResult<FiscalDocumentDetails>(
            items,
            query.Page,
            query.PageSize,
            totalItems);
    }

    private async Task<FiscalDocumentMongoModel?>
        FindMongoModelByIdAsync(
            Guid id,
            string tenantId,
            CancellationToken cancellationToken)
    {
        var idAsString = id.ToString();

        return await _collection
            .Find(document =>
                document.Id == idAsString
                && document.TenantId == tenantId)
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
            FailureReason =
                document.FailureReason
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
            ToDateTimeOffset(
                mongoModel.ReceivedAtUtc),
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