using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
using FiscalFlow.Infrastructure.MongoDb;
using MongoDB.Driver;

namespace FiscalFlow.Infrastructure.Documents;

public sealed class FiscalDocumentRepository
    : IFiscalDocumentRepository
{
    private const string CollectionName = "fiscalDocuments";

    private readonly IMongoCollection<FiscalDocumentMongoModel>
        _collection;

    public FiscalDocumentRepository(MongoDbContext mongoDbContext)
    {
        _collection = mongoDbContext
            .GetCollection<FiscalDocumentMongoModel>(CollectionName);
    }

    public async Task InsertAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        try
        {
            await _collection.InsertOneAsync(
                MapToMongoModel(document),
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
        var model = await FindMongoModelByIdAsync(
            id,
            tenantId,
            cancellationToken);

        return model is null ? null : MapToDetails(model);
    }

    public async Task<FiscalDocumentDetails?> FindByExternalDocumentIdAsync(
        string tenantId,
        string externalDocumentId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalDocumentId);

        var model = await _collection
            .Find(document =>
                document.TenantId == tenantId.Trim()
                && document.ExternalDocumentId == externalDocumentId.Trim())
            .FirstOrDefaultAsync(cancellationToken);

        return model is null ? null : MapToDetails(model);
    }

    public async Task<FiscalDocument?> FindDomainByIdAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var model = await FindMongoModelByIdAsync(
            id,
            tenantId,
            cancellationToken);

        return model is null ? null : MapToDomain(model);
    }

    public async Task<FiscalDocument?> TryStartProcessingAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        var filterBuilder = Builders<FiscalDocumentMongoModel>.Filter;
        var filter = filterBuilder.And(
            filterBuilder.Eq(document => document.Id, id.ToString()),
            filterBuilder.Eq(document => document.TenantId, tenantId.Trim()),
            filterBuilder.In(
                document => document.Status,
                [
                    DocumentProcessingStatus.Received.ToString(),
                    DocumentProcessingStatus.Failed.ToString()
                ]));

        var updateBuilder = Builders<FiscalDocumentMongoModel>.Update;
        var update = updateBuilder.Combine(
            updateBuilder.Set(
                document => document.Status,
                DocumentProcessingStatus.Processing.ToString()),
            updateBuilder.Unset(document => document.ProcessedAtUtc),
            updateBuilder.Unset(document => document.FailureReason),
            updateBuilder.Unset(document => document.AccessKey),
            updateBuilder.Unset(document => document.IssuerDocument),
            updateBuilder.Unset(document => document.IssuerName),
            updateBuilder.Unset(document => document.RecipientDocument),
            updateBuilder.Unset(document => document.RecipientName),
            updateBuilder.Unset(document => document.TotalValue),
            updateBuilder.Unset(document => document.IssuedAt));

        var model = await _collection.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<
                FiscalDocumentMongoModel,
                FiscalDocumentMongoModel>
            {
                ReturnDocument = ReturnDocument.After
            },
            cancellationToken);

        return model is null ? null : MapToDomain(model);
    }

    public async Task<IReadOnlyCollection<FiscalDocument>>
        ClaimFailedForReprocessingAsync(
            int maximumAttempts,
            int batchSize,
            CancellationToken cancellationToken = default)
    {
        if (maximumAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumAttempts),
                "O limite de tentativas deve ser maior que zero.");
        }

        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize),
                "O tamanho do lote deve ser maior que zero.");
        }

        var claimedDocuments = new List<FiscalDocument>(batchSize);
        var filterBuilder = Builders<FiscalDocumentMongoModel>.Filter;

        var attemptsFilter = filterBuilder.Or(
            filterBuilder.Exists(
                document => document.ReprocessingAttempts,
                false),
            filterBuilder.Lt(
                document => document.ReprocessingAttempts,
                maximumAttempts));

        var filter = filterBuilder.And(
            filterBuilder.Eq(
                document => document.Status,
                DocumentProcessingStatus.Failed.ToString()),
            attemptsFilter);

        var updateBuilder = Builders<FiscalDocumentMongoModel>.Update;
        var update = updateBuilder.Combine(
            updateBuilder.Set(
                document => document.Status,
                DocumentProcessingStatus.Received.ToString()),
            updateBuilder.Inc(
                document => document.ReprocessingAttempts,
                1),
            updateBuilder.Set(
                document => document.LastReprocessingAtUtc,
                DateTime.UtcNow),
            updateBuilder.Unset(document => document.ProcessedAtUtc),
            updateBuilder.Unset(document => document.FailureReason),
            updateBuilder.Unset(document => document.AccessKey),
            updateBuilder.Unset(document => document.IssuerDocument),
            updateBuilder.Unset(document => document.IssuerName),
            updateBuilder.Unset(document => document.RecipientDocument),
            updateBuilder.Unset(document => document.RecipientName),
            updateBuilder.Unset(document => document.TotalValue),
            updateBuilder.Unset(document => document.IssuedAt));

        var options = new FindOneAndUpdateOptions<
            FiscalDocumentMongoModel,
            FiscalDocumentMongoModel>
        {
            ReturnDocument = ReturnDocument.After,
            Sort = Builders<FiscalDocumentMongoModel>.Sort
                .Ascending(document => document.ReceivedAtUtc)
        };

        for (var index = 0; index < batchSize; index++)
        {
            var model = await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                options,
                cancellationToken);

            if (model is null)
            {
                break;
            }

            claimedDocuments.Add(MapToDomain(model));
        }

        return claimedDocuments;
    }

    public async Task UpdateAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        var model = MapToMongoModel(document);
        var result = await _collection.ReplaceOneAsync(
            stored =>
                stored.Id == model.Id
                && stored.TenantId == model.TenantId,
            model,
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException(
                $"O documento {document.Id} não foi encontrado durante a atualização.");
        }
    }

    public async Task<PagedResult<FiscalDocumentDetails>> ListAsync(
        ListFiscalDocumentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var builder = Builders<FiscalDocumentMongoModel>.Filter;
        var filters = new List<FilterDefinition<FiscalDocumentMongoModel>>
        {
            builder.Eq(document => document.TenantId, query.TenantId)
        };

        if (query.Status is not null)
        {
            filters.Add(
                builder.Eq(
                    document => document.Status,
                    query.Status.Value.ToString()));
        }

        var filter = builder.And(filters);
        var totalItems = await _collection.CountDocumentsAsync(
            filter,
            cancellationToken: cancellationToken);

        var models = await _collection
            .Find(filter)
            .SortByDescending(document => document.ReceivedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<FiscalDocumentDetails>(
            models.Select(MapToDetails).ToList(),
            query.Page,
            query.PageSize,
            totalItems);
    }

    private async Task<FiscalDocumentMongoModel?> FindMongoModelByIdAsync(
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
            ExternalDocumentId = document.ExternalDocumentId,
            XmlContent = document.XmlContent,
            AccessKey = document.FiscalData?.AccessKey,
            IssuerDocument = document.FiscalData?.IssuerDocument,
            IssuerName = document.FiscalData?.IssuerName,
            RecipientDocument = document.FiscalData?.RecipientDocument,
            RecipientName = document.FiscalData?.RecipientName,
            TotalValue = document.FiscalData?.TotalValue,
            IssuedAt = document.FiscalData?.IssuedAt.UtcDateTime,
            Status = document.Status.ToString(),
            ReceivedAtUtc = document.ReceivedAtUtc.UtcDateTime,
            ProcessedAtUtc = document.ProcessedAtUtc?.UtcDateTime,
            FailureReason = document.FailureReason,
            ReprocessingAttempts = document.ReprocessingAttempts,
            LastReprocessingAtUtc =
                document.LastReprocessingAtUtc?.UtcDateTime
        };
    }

    private static FiscalDocument MapToDomain(
        FiscalDocumentMongoModel model)
    {
        if (!Enum.TryParse<DocumentProcessingStatus>(
                model.Status,
                ignoreCase: true,
                out var status))
        {
            throw new InvalidOperationException(
                $"O status '{model.Status}' armazenado no MongoDB é inválido.");
        }

        return FiscalDocument.Rehydrate(
            Guid.Parse(model.Id),
            model.TenantId,
            model.ExternalDocumentId,
            status,
            ToDateTimeOffset(model.ReceivedAtUtc),
            model.ProcessedAtUtc is null
                ? null
                : ToDateTimeOffset(model.ProcessedAtUtc.Value),
            model.FailureReason,
            model.XmlContent,
            MapToFiscalData(model),
            model.ReprocessingAttempts,
            model.LastReprocessingAtUtc is null
                ? null
                : ToDateTimeOffset(model.LastReprocessingAtUtc.Value));
    }

    private static FiscalDocumentDetails MapToDetails(
        FiscalDocumentMongoModel model)
    {
        var fiscalData = MapToFiscalData(model);

        return new FiscalDocumentDetails(
            Guid.Parse(model.Id),
            model.TenantId,
            model.ExternalDocumentId,
            model.Status,
            ToDateTimeOffset(model.ReceivedAtUtc),
            model.ProcessedAtUtc is null
                ? null
                : ToDateTimeOffset(model.ProcessedAtUtc.Value),
            model.FailureReason,
            fiscalData is null
                ? null
                : MapToDetails(fiscalData));
    }

    private static FiscalDocumentDataDetails MapToDetails(
        FiscalDocumentData data)
    {
        return new FiscalDocumentDataDetails(
            data.AccessKey,
            data.IssuerDocument,
            data.IssuerName,
            data.RecipientDocument,
            data.RecipientName,
            data.TotalValue,
            data.IssuedAt);
    }

    private static FiscalDocumentData? MapToFiscalData(
        FiscalDocumentMongoModel model)
    {
        if (string.IsNullOrWhiteSpace(model.AccessKey)
            || string.IsNullOrWhiteSpace(model.IssuerDocument)
            || string.IsNullOrWhiteSpace(model.IssuerName)
            || string.IsNullOrWhiteSpace(model.RecipientDocument)
            || string.IsNullOrWhiteSpace(model.RecipientName)
            || model.TotalValue is null
            || model.IssuedAt is null)
        {
            return null;
        }

        return new FiscalDocumentData(
            model.AccessKey,
            model.IssuerDocument,
            model.IssuerName,
            model.RecipientDocument,
            model.RecipientName,
            model.TotalValue.Value,
            ToDateTimeOffset(model.IssuedAt.Value));
    }

    private static DateTimeOffset ToDateTimeOffset(DateTime value)
    {
        return new DateTimeOffset(
            DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
