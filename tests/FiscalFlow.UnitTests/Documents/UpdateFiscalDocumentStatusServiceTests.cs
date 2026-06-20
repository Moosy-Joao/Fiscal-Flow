using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
using FiscalFlow.UnitTests.Fakes;

namespace FiscalFlow.UnitTests.Documents;

public sealed class UpdateFiscalDocumentStatusServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldMoveReceivedDocumentToProcessing()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var document = new FiscalDocument(
            "empresa-demo",
            "NFE-004");

        await repository.InsertAsync(document);

        var service =
            new UpdateFiscalDocumentStatusService(
                repository);

        var command =
            new UpdateFiscalDocumentStatusCommand(
                document.Id,
                DocumentProcessingStatus.Processing,
                null);

        var result = await service.ExecuteAsync(command);

        Assert.NotNull(result);
        Assert.Equal("Processing", result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMoveProcessingDocumentToProcessed()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var document = new FiscalDocument(
            "empresa-demo",
            "NFE-005");

        document.MarkAsProcessing();

        await repository.InsertAsync(document);

        var service =
            new UpdateFiscalDocumentStatusService(
                repository);

        var command =
            new UpdateFiscalDocumentStatusCommand(
                document.Id,
                DocumentProcessingStatus.Processed,
                null);

        var result = await service.ExecuteAsync(command);

        Assert.NotNull(result);
        Assert.Equal("Processed", result.Status);
        Assert.NotNull(result.ProcessedAtUtc);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var service =
            new UpdateFiscalDocumentStatusService(
                repository);

        var command =
            new UpdateFiscalDocumentStatusCommand(
                Guid.NewGuid(),
                DocumentProcessingStatus.Processing,
                null);

        var result = await service.ExecuteAsync(command);

        Assert.Null(result);
    }
}