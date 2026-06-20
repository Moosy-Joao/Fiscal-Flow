using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
using FiscalFlow.UnitTests.Fakes;

namespace FiscalFlow.UnitTests.Documents;

public sealed class CreateFiscalDocumentServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPersistReceivedDocument()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var service =
            new CreateFiscalDocumentService(repository);

        var command = new CreateFiscalDocumentCommand(
            "empresa-demo",
            "NFE-002");

        var result = await service.ExecuteAsync(
            command,
            CancellationToken.None);

        var savedDocument =
            Assert.Single(repository.Documents);

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
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnExistingDocument_WhenRepeated()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var service =
            new CreateFiscalDocumentService(repository);

        var command = new CreateFiscalDocumentCommand(
            "empresa-demo",
            "NFE-IDEMPOTENTE");

        var firstResult =
            await service.ExecuteAsync(command);

        var secondResult =
            await service.ExecuteAsync(command);

        var savedDocument =
            Assert.Single(repository.Documents);

        Assert.True(firstResult.WasCreated);
        Assert.False(secondResult.WasCreated);

        Assert.Equal(
            firstResult.Id,
            secondResult.Id);

        Assert.Equal(
            savedDocument.Id,
            secondResult.Id);
    }
}