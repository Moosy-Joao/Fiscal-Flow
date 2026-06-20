using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;

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
    }

    private sealed class FakeFiscalDocumentRepository
        : IFiscalDocumentRepository
    {
        public List<FiscalDocument> Documents { get; } =
            [];

        public Task InsertAsync(
            FiscalDocument document,
            CancellationToken cancellationToken = default)
        {
            Documents.Add(document);
            return Task.CompletedTask;
        }
    }
}