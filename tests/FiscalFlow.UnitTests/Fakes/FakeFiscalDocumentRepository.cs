using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;

namespace FiscalFlow.UnitTests.Fakes;

internal sealed class FakeFiscalDocumentRepository
    : IFiscalDocumentRepository
{
    public List<FiscalDocument> Documents { get; } = [];

    public Task InsertAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default)
    {
        var alreadyExists = Documents.Any(
            savedDocument =>
                savedDocument.TenantId == document.TenantId
                && savedDocument.ExternalDocumentId
                    == document.ExternalDocumentId);

        if (alreadyExists)
        {
            throw new DuplicateFiscalDocumentException(
                document.TenantId,
                document.ExternalDocumentId);
        }

        Documents.Add(document);
        return Task.CompletedTask;
    }

    public Task<FiscalDocumentDetails?> FindByIdAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var document = Documents.SingleOrDefault(
            item => item.Id == id
                && item.TenantId == tenantId);

        return Task.FromResult(
            document is null ? null : MapToDetails(document));
    }

    public Task<FiscalDocument?> FindDomainByIdAsync(
        Guid id,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var document = Documents.SingleOrDefault(
            item => item.Id == id
                && item.TenantId == tenantId);

        return Task.FromResult(document);
    }

    public Task UpdateAsync(
        FiscalDocument document,
        CancellationToken cancellationToken = default)
    {
        var index = Documents.FindIndex(
            item => item.Id == document.Id
                && item.TenantId == document.TenantId);

        if (index < 0)
        {
            throw new InvalidOperationException(
                "Documento não encontrado.");
        }

        Documents[index] = document;
        return Task.CompletedTask;
    }

    public Task<PagedResult<FiscalDocumentDetails>> ListAsync(
        ListFiscalDocumentsQuery query,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<FiscalDocument> filtered = Documents.Where(
            document => document.TenantId == query.TenantId);

        if (query.Status is not null)
        {
            filtered = filtered.Where(
                document => document.Status == query.Status.Value);
        }

        var totalItems = filtered.LongCount();
        var items = filtered
            .OrderByDescending(document => document.ReceivedAtUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToDetails)
            .ToList();

        return Task.FromResult(
            new PagedResult<FiscalDocumentDetails>(
                items,
                query.Page,
                query.PageSize,
                totalItems));
    }

    public Task<FiscalDocumentDetails?>
        FindByExternalDocumentIdAsync(
            string tenantId,
            string externalDocumentId,
            CancellationToken cancellationToken = default)
    {
        var document = Documents.SingleOrDefault(
            item => item.TenantId == tenantId
                && item.ExternalDocumentId == externalDocumentId);

        return Task.FromResult(
            document is null ? null : MapToDetails(document));
    }

    private static FiscalDocumentDetails MapToDetails(
        FiscalDocument document)
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
