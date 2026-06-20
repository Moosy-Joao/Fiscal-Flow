using FiscalFlow.Domain.Documents;

namespace FiscalFlow.UnitTests.Documents;

public sealed class FiscalDocumentTests
{
    [Fact]
    public void Constructor_ShouldCreateReceivedDocument()
    {
        var receivedAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var document = new FiscalDocument("tenant-a", "NFE-123", receivedAt);

        Assert.NotEqual(Guid.Empty, document.Id);
        Assert.Equal("tenant-a", document.TenantId);
        Assert.Equal("NFE-123", document.ExternalDocumentId);
        Assert.Equal(DocumentProcessingStatus.Received, document.Status);
        Assert.Equal(receivedAt, document.ReceivedAtUtc);
    }

    [Fact]
    public void MarkAsProcessed_ShouldRequireProcessingStatus()
    {
        var document = new FiscalDocument("tenant-a", "NFE-123");

        Assert.Throws<InvalidOperationException>(() => document.MarkAsProcessed());
    }

    [Fact]
    public void ProcessingFlow_ShouldFinishDocument()
    {
        var processedAt = new DateTimeOffset(2026, 6, 20, 13, 0, 0, TimeSpan.Zero);
        var document = new FiscalDocument("tenant-a", "NFE-123");

        document.MarkAsProcessing();
        document.MarkAsProcessed(processedAt);

        Assert.Equal(DocumentProcessingStatus.Processed, document.Status);
        Assert.Equal(processedAt, document.ProcessedAtUtc);
    }
}
