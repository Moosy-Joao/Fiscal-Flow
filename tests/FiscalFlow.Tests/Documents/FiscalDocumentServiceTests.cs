using System.Text.Json;
using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Tests.Documents;

public sealed class FiscalDocumentServiceTests
{
    [Fact]
    public async Task SubmitAsync_ShouldReturnDuplicate_WhenIdempotencyAlreadyExists()
    {
        var docs = new FakeFiscalDocumentRepository();
        var idempotency = new FakeIdempotencyRepository
        {
            Existing = new IdempotencyRecord
            {
                TenantId = "tenant-a",
                IdempotencyKey = "dup-key",
                DocumentId = "doc-existing"
            }
        };
        var queue = new FakeDispatchQueue();
        var jobs = new FakeJobScheduler();

        var sut = new FiscalDocumentService(docs, idempotency, queue, jobs);

        var result = await sut.SubmitAsync(
            "tenant-a",
            "dup-key",
            new SubmitFiscalDocumentRequest
            {
                ExternalDocumentId = "NFE-1",
                Payload = JsonDocument.Parse("{\"value\":1}").RootElement
            },
            CancellationToken.None);

        Assert.True(result.IsDuplicate);
        Assert.Equal("doc-existing", result.DocumentId);
        Assert.False(queue.WasCalled);
        Assert.False(jobs.WasCalled);
    }

    [Fact]
    public async Task SubmitAsync_ShouldPersistAndDispatch_WhenNewRequest()
    {
        var docs = new FakeFiscalDocumentRepository();
        var idempotency = new FakeIdempotencyRepository();
        var queue = new FakeDispatchQueue();
        var jobs = new FakeJobScheduler();

        var sut = new FiscalDocumentService(docs, idempotency, queue, jobs);

        var result = await sut.SubmitAsync(
            "tenant-a",
            "new-key",
            new SubmitFiscalDocumentRequest
            {
                ExternalDocumentId = "NFE-2",
                Payload = JsonDocument.Parse("{\"value\":2}").RootElement
            },
            CancellationToken.None);

        Assert.False(result.IsDuplicate);
        Assert.Single(docs.Inserted);
        Assert.True(queue.WasCalled);
        Assert.True(jobs.WasCalled);
    }

    private sealed class FakeFiscalDocumentRepository : IFiscalDocumentRepository
    {
        public List<FiscalDocument> Inserted { get; } = new();

        public Task<FiscalDocument?> GetByIdAsync(string tenantId, string documentId, CancellationToken cancellationToken)
            => Task.FromResult<FiscalDocument?>(Inserted.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId));

        public Task InsertAsync(FiscalDocument document, CancellationToken cancellationToken)
        {
            Inserted.Add(document);
            return Task.CompletedTask;
        }

        public Task UpdateStatusAsync(string tenantId, string documentId, DocumentProcessingStatus status, DateTimeOffset? processedAtUtc, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class FakeIdempotencyRepository : IIdempotencyRepository
    {
        public IdempotencyRecord? Existing { get; set; }

        public Task<IdempotencyRecord?> GetAsync(string tenantId, string idempotencyKey, CancellationToken cancellationToken)
            => Task.FromResult(Existing is not null && Existing.TenantId == tenantId && Existing.IdempotencyKey == idempotencyKey
                ? Existing
                : null);

        public Task<bool> TryInsertAsync(IdempotencyRecord record, CancellationToken cancellationToken)
        {
            Existing = record;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeDispatchQueue : IDocumentDispatchQueue
    {
        public bool WasCalled { get; private set; }

        public Task PublishAsync(string tenantId, string documentId, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeJobScheduler : IBackgroundJobScheduler
    {
        public bool WasCalled { get; private set; }

        public void EnqueueProcessing(string tenantId, string documentId)
        {
            WasCalled = true;
        }
    }
}
