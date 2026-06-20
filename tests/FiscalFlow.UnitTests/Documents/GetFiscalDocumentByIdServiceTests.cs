using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
using FiscalFlow.UnitTests.Fakes;

namespace FiscalFlow.UnitTests.Documents;

public sealed class GetFiscalDocumentByIdServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnDocument_WhenItExists()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var document = new FiscalDocument(
            "empresa-demo",
            "NFE-003");

        await repository.InsertAsync(document);

        var service =
            new GetFiscalDocumentByIdService(repository);

        var result = await service.ExecuteAsync(
            document.Id,
            "empresa-demo",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(document.Id, result.Id);
        Assert.Equal(
            "empresa-demo",
            result.TenantId);
        Assert.Equal(
            "NFE-003",
            result.ExternalDocumentId);
        Assert.Equal(
            "Received",
            result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNull_WhenItDoesNotExist()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var service =
            new GetFiscalDocumentByIdService(repository);

        var result = await service.ExecuteAsync(
            Guid.NewGuid(),
            "empresa-demo",
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotReturnDocumentFromAnotherTenant()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var document = new FiscalDocument(
            "empresa-a",
            "NFE-SEGURA");

        await repository.InsertAsync(document);

        var service =
            new GetFiscalDocumentByIdService(repository);

        var result = await service.ExecuteAsync(
            document.Id,
            "empresa-b",
            CancellationToken.None);

        Assert.Null(result);
    }
}
