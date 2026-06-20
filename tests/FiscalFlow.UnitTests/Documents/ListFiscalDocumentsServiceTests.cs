using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
using FiscalFlow.UnitTests.Fakes;

namespace FiscalFlow.UnitTests.Documents;

public sealed class ListFiscalDocumentsServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldFilterByTenantAndStatus()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var receivedDocument = new FiscalDocument(
            "empresa-a",
            "NFE-101");

        var processingDocument = new FiscalDocument(
            "empresa-a",
            "NFE-102");

        processingDocument.MarkAsProcessing();

        var otherTenantDocument = new FiscalDocument(
            "empresa-b",
            "NFE-103");

        otherTenantDocument.MarkAsProcessing();

        await repository.InsertAsync(receivedDocument);
        await repository.InsertAsync(processingDocument);
        await repository.InsertAsync(otherTenantDocument);

        var service =
            new ListFiscalDocumentsService(repository);

        var query = new ListFiscalDocumentsQuery(
            "empresa-a",
            DocumentProcessingStatus.Processing,
            1,
            10);

        var result = await service.ExecuteAsync(query);

        var item = Assert.Single(result.Items);

        Assert.Equal("NFE-102", item.ExternalDocumentId);
        Assert.Equal("Processing", item.Status);
        Assert.Equal(1, result.TotalItems);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnRequestedPage()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        for (var index = 1; index <= 5; index++)
        {
            var receivedAt = new DateTimeOffset(
                2026,
                6,
                20,
                12,
                index,
                0,
                TimeSpan.Zero);

            var document = new FiscalDocument(
                "empresa-a",
                $"NFE-{index}",
                receivedAt);

            await repository.InsertAsync(document);
        }

        var service =
            new ListFiscalDocumentsService(repository);

        var query = new ListFiscalDocumentsQuery(
            "empresa-a",
            null,
            2,
            2);

        var result = await service.ExecuteAsync(query);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRejectInvalidPageSize()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var service =
            new ListFiscalDocumentsService(repository);

        var query = new ListFiscalDocumentsQuery(
            "empresa-a",
            null,
            1,
            101);

        await Assert.ThrowsAsync<
            ArgumentOutOfRangeException>(
            () => service.ExecuteAsync(query));
    }
}