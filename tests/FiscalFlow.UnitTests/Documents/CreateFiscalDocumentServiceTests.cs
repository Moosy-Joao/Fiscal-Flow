using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
using FiscalFlow.UnitTests.Fakes;

namespace FiscalFlow.UnitTests.Documents;

public sealed class CreateFiscalDocumentServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPersistAndPublishReceivedDocument()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var publisher =
            new FakeFiscalDocumentReceivedPublisher();

        var service =
            new CreateFiscalDocumentService(
                repository,
                publisher);

        var command = new CreateFiscalDocumentCommand(
            "empresa-demo",
            "NFE-002");

        var result = await service.ExecuteAsync(
            command,
            CancellationToken.None);

        var savedDocument =
            Assert.Single(repository.Documents);

        var publishedMessage =
            Assert.Single(publisher.Messages);

        Assert.Equal(
            DocumentProcessingStatus.Received,
            savedDocument.Status);

        Assert.Equal(
            "empresa-demo",
            savedDocument.TenantId);

        Assert.Equal(
            "NFE-002",
            savedDocument.ExternalDocumentId);

        Assert.Equal(savedDocument.Id, result.Id);
        Assert.Equal("Received", result.Status);
        Assert.True(result.WasCreated);

        Assert.Equal(
            savedDocument.Id,
            publishedMessage.DocumentId);

        Assert.Equal(
            savedDocument.TenantId,
            publishedMessage.TenantId);

        Assert.Equal(
            savedDocument.ExternalDocumentId,
            publishedMessage.ExternalDocumentId);

        Assert.Equal(
            savedDocument.ReceivedAtUtc,
            publishedMessage.ReceivedAtUtc);

        Assert.NotEqual(
            Guid.Empty,
            publishedMessage.CorrelationId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotPublishAgain_WhenRepeated()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var publisher =
            new FakeFiscalDocumentReceivedPublisher();

        var service =
            new CreateFiscalDocumentService(
                repository,
                publisher);

        var command = new CreateFiscalDocumentCommand(
            "empresa-demo",
            "NFE-IDEMPOTENTE");

        var firstResult =
            await service.ExecuteAsync(command);

        var secondResult =
            await service.ExecuteAsync(command);

        var savedDocument =
            Assert.Single(repository.Documents);

        var publishedMessage =
            Assert.Single(publisher.Messages);

        Assert.True(firstResult.WasCreated);
        Assert.False(secondResult.WasCreated);

        Assert.Equal(
            firstResult.Id,
            secondResult.Id);

        Assert.Equal(
            savedDocument.Id,
            secondResult.Id);

        Assert.Equal(
            firstResult.Id,
            publishedMessage.DocumentId);
    }
}