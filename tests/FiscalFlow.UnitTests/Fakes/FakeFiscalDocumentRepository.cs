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
        Documents.Add(document);

        return Task.CompletedTask;
    }

    public Task<FiscalDocumentDetails?> FindByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var document = Documents.SingleOrDefault(
            item => item.Id == id);

        if (document is null)
        {
            return Task.FromResult<
                FiscalDocumentDetails?>(null);
        }

        var details = new FiscalDocumentDetails(
            document.Id,
            document.TenantId,
            document.ExternalDocumentId,
            document.Status.ToString(),
            document.ReceivedAtUtc,
            document.ProcessedAtUtc,
            document.FailureReason);

        return Task.FromResult<
            FiscalDocumentDetails?>(details);
    }
}