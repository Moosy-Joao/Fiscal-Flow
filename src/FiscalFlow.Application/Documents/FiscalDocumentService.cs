using System.Diagnostics.Metrics;
using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

public sealed class FiscalDocumentService : IFiscalDocumentService
{
    private static readonly Meter Meter = new("FiscalFlow.Ingestion");
    private static readonly Counter<long> ReceivedCounter = Meter.CreateCounter<long>("fiscal_documents_received_total");
    private static readonly Counter<long> DuplicateCounter = Meter.CreateCounter<long>("fiscal_documents_duplicate_total");

    private readonly IFiscalDocumentRepository _documents;
    private readonly IIdempotencyRepository _idempotency;
    private readonly IDocumentDispatchQueue _dispatchQueue;
    private readonly IBackgroundJobScheduler _backgroundJobs;

    public FiscalDocumentService(
        IFiscalDocumentRepository documents,
        IIdempotencyRepository idempotency,
        IDocumentDispatchQueue dispatchQueue,
        IBackgroundJobScheduler backgroundJobs)
    {
        _documents = documents;
        _idempotency = idempotency;
        _dispatchQueue = dispatchQueue;
        _backgroundJobs = backgroundJobs;
    }

    public async Task<SubmitFiscalDocumentResult> SubmitAsync(string tenantId, string idempotencyKey, SubmitFiscalDocumentRequest request, CancellationToken cancellationToken)
    {
        var existing = await _idempotency.GetAsync(tenantId, idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            DuplicateCounter.Add(1);
            return new SubmitFiscalDocumentResult(existing.DocumentId, true);
        }

        var document = new FiscalDocument
        {
            TenantId = tenantId,
            ExternalDocumentId = request.ExternalDocumentId,
            PayloadJson = request.Payload.GetRawText(),
            Status = DocumentProcessingStatus.Received
        };

        await _documents.InsertAsync(document, cancellationToken);

        var insertSucceeded = await _idempotency.TryInsertAsync(
            new IdempotencyRecord
            {
                TenantId = tenantId,
                IdempotencyKey = idempotencyKey,
                DocumentId = document.Id
            },
            cancellationToken);

        if (!insertSucceeded)
        {
            var winner = await _idempotency.GetAsync(tenantId, idempotencyKey, cancellationToken);
            var winnerDocumentId = winner?.DocumentId ?? document.Id;
            DuplicateCounter.Add(1);
            return new SubmitFiscalDocumentResult(winnerDocumentId, true);
        }

        await _dispatchQueue.PublishAsync(tenantId, document.Id, cancellationToken);
        _backgroundJobs.EnqueueProcessing(tenantId, document.Id);

        ReceivedCounter.Add(1);
        return new SubmitFiscalDocumentResult(document.Id, false);
    }
}
