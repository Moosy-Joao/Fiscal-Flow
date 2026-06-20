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
        Assert.Null(result.FiscalData);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFiscalData_WhenProcessed()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var document = new FiscalDocument(
            "empresa-demo",
            "NFE-COM-DADOS");

        var fiscalData = new FiscalDocumentData(
            "41260612345678000195550010000012341000012345",
            "12345678000195",
            "Empresa Emitente Ltda",
            "12345678901",
            "Cliente Destinatário",
            1500.75M,
            new DateTimeOffset(
                2026,
                6,
                20,
                10,
                30,
                0,
                TimeSpan.FromHours(-3)));

        document.MarkAsProcessing();
        document.CompleteProcessing(fiscalData);

        await repository.InsertAsync(document);

        var service =
            new GetFiscalDocumentByIdService(repository);

        var result = await service.ExecuteAsync(
            document.Id,
            document.TenantId,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.FiscalData);
        Assert.Equal(
            fiscalData.AccessKey,
            result.FiscalData.AccessKey);
        Assert.Equal(
            fiscalData.TotalValue,
            result.FiscalData.TotalValue);
        Assert.Equal(
            fiscalData.IssuedAt,
            result.FiscalData.IssuedAt);
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
