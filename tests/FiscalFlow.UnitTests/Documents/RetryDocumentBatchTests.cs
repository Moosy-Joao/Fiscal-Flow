using FiscalFlow.Application.Documents;
using FiscalFlow.Application.Documents.Xml;
using FiscalFlow.Domain.Documents;
using FiscalFlow.UnitTests.Fakes;

namespace FiscalFlow.UnitTests.Documents;

public sealed class RetryDocumentBatchTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldProcessEligibleDocument()
    {
        var repository = new FakeFiscalDocumentRepository();
        var service = CreateService(repository);
        var document = new FiscalDocument(
            "empresa-demo",
            "NFE-RETRY-SUCESSO",
            xmlContent: ValidXml);

        document.MarkAsFailed("Falha temporária.");
        await repository.InsertAsync(document);

        var result = await service.ExecuteAsync(
            new FailedDocumentBatchCommand(3, 10));

        Assert.Equal(1, result.ClaimedCount);
        Assert.Equal(1, result.ProcessedCount);
        Assert.Equal(0, result.FailedCount);
        Assert.Equal(1, document.ReprocessingAttempts);
        Assert.NotNull(document.LastReprocessingAtUtc);
        Assert.Equal(DocumentProcessingStatus.Processed, document.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRespectMaximumAttempts()
    {
        var repository = new FakeFiscalDocumentRepository();
        var service = CreateService(repository);
        var document = new FiscalDocument(
            "empresa-demo",
            "NFE-RETRY-LIMITE",
            xmlContent: "<xml-invalido>");

        document.MarkAsFailed("Falha inicial.");
        await repository.InsertAsync(document);

        var command = new FailedDocumentBatchCommand(2, 10);

        await service.ExecuteAsync(command);
        await service.ExecuteAsync(command);
        var lastResult = await service.ExecuteAsync(command);

        Assert.Equal(0, lastResult.ClaimedCount);
        Assert.Equal(2, document.ReprocessingAttempts);
        Assert.Equal(DocumentProcessingStatus.Failed, document.Status);
    }

    private static RetryDocumentBatchService CreateService(
        FakeFiscalDocumentRepository repository)
    {
        var processService = new ProcessFiscalDocumentService(
            repository,
            new FiscalDocumentXmlParser());

        return new RetryDocumentBatchService(
            repository,
            processService);
    }

    private const string ValidXml =
        """
        <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe">
          <NFe>
            <infNFe Id="NFe41260612345678000195550010000012341000012345">
              <ide><dhEmi>2026-06-20T10:30:00-03:00</dhEmi></ide>
              <emit><CNPJ>12345678000195</CNPJ><xNome>Emitente</xNome></emit>
              <dest><CPF>12345678901</CPF><xNome>Destinatário</xNome></dest>
              <total><ICMSTot><vNF>1500.75</vNF></ICMSTot></total>
            </infNFe>
          </NFe>
        </nfeProc>
        """;
}
