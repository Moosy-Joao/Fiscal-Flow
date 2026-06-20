using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;

namespace FiscalFlow.IntegrationTests.Fakes;

public sealed class InMemoryFiscalDocumentRepository :
    IFiscalDocumentRepository
{
    private readonly List<FiscalDocument> _documents = [];
    private readonly object _lock = new();

    public IReadOnlyCollection<FiscalDocument> Documents
    {
        get
        {
            lock (_lock)
            {
                return _documents.ToArray();
            }
        }
    }

    public Task InsertAsync(FiscalDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        lock (_lock)
        {
            if (_documents.Any(item => item.TenantId == document.TenantId && item.ExternalDocumentId == document.ExternalDocumentId))
            {
                throw new DuplicateFiscalDocumentException(document.TenantId, document.ExternalDocumentId);
            }
            _documents.Add(document);
        }
        return Task.CompletedTask;
    }

    public Task<FiscalDocumentDetails?> FindByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var document = _documents.SingleOrDefault(item => item.Id == id && item.TenantId == tenantId);
            return Task.FromResult(document is null ? null : MapToDetails(document));
        }
    }

    public Task<FiscalDocumentDetails?> FindByExternalDocumentIdAsync(string tenantId, string externalDocumentId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var document = _documents.SingleOrDefault(item => item.TenantId == tenantId && item.ExternalDocumentId == externalDocumentId);
            return Task.FromResult(document is null ? null : MapToDetails(document));
        }
    }

    public Task<FiscalDocument?> FindDomainByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_documents.SingleOrDefault(item => item.Id == id && item.TenantId == tenantId));
        }
    }

    public Task<FiscalDocument?> TryStartProcessingAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var document = _documents.SingleOrDefault(item => item.Id == id && item.TenantId == tenantId);
            if (document is null || document.Status is not DocumentProcessingStatus.Received and not DocumentProcessingStatus.Failed)
            {
                return Task.FromResult<FiscalDocument?>(null);
            }
            document.MarkAsProcessing();
            return Task.FromResult<FiscalDocument?>(document);
        }
    }

    public Task<IReadOnlyCollection<FiscalDocument>> ClaimFailedForReprocessingAsync(int maximumAttempts, int batchSize, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var documents = _documents
                .Where(document => document.Status == DocumentProcessingStatus.Failed && document.ReprocessingAttempts < maximumAttempts)
                .OrderBy(document => document.ReceivedAtUtc)
                .Take(batchSize)
                .ToList();

            foreach (var document in documents)
            {
                document.PrepareForReprocessing();
            }

            return Task.FromResult<IReadOnlyCollection<FiscalDocument>>(documents);
        }
    }

    public Task UpdateAsync(FiscalDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        lock (_lock)
        {
            var index = _documents.FindIndex(item => item.Id == document.Id && item.TenantId == document.TenantId);
            if (index < 0)
            {
                throw new InvalidOperationException("Documento não encontrado.");
            }
            _documents[index] = document;
        }
        return Task.CompletedTask;
    }

    public Task<PagedResult<FiscalDocumentDetails>> ListAsync(ListFiscalDocumentsQuery query, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IEnumerable<FiscalDocument> filtered = _documents.Where(document => document.TenantId == query.TenantId);
            if (query.Status is not null)
            {
                filtered = filtered.Where(document => document.Status == query.Status.Value);
            }

            var totalItems = filtered.LongCount();
            var items = filtered
                .OrderByDescending(document => document.ReceivedAtUtc)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(MapToDetails)
                .ToList();

            return Task.FromResult(new PagedResult<FiscalDocumentDetails>(items, query.Page, query.PageSize, totalItems));
        }
    }

    private static FiscalDocumentDetails MapToDetails(FiscalDocument document)
    {
        var data = document.FiscalData;
        return new FiscalDocumentDetails(
            document.Id,
            document.TenantId,
            document.ExternalDocumentId,
            document.Status.ToString(),
            document.ReceivedAtUtc,
            document.ProcessedAtUtc,
            document.FailureReason,
            data is null
                ? null
                : new FiscalDocumentDataDetails(
                    data.AccessKey,
                    data.IssuerDocument,
                    data.IssuerName,
                    data.RecipientDocument,
                    data.RecipientName,
                    data.TotalValue,
                    data.IssuedAt));
    }
}
