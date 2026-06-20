using FiscalFlow.Application.Documents;
using FiscalFlow.Application.Documents.Xml;
using FiscalFlow.Domain.Documents;
using FiscalFlow.UnitTests.Fakes;

namespace FiscalFlow.UnitTests.Documents;

public sealed class ProcessFiscalDocumentServiceTests
{
    private const string ValidXml =
        """
        <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe">
          <NFe>
            <infNFe Id="NFe41260612345678000195550010000012341000012345">
              <ide>
                <dhEmi>2026-06-20T10:30:00-03:00</dhEmi>
              </ide>
              <emit>
                <CNPJ>12345678000195</CNPJ>
                <xNome>Empresa Emitente Ltda</xNome>
              </emit>
              <dest>
                <CPF>12345678901</CPF>
                <xNome>Cliente Destinatário</xNome>
              </dest>
              <total>
                <ICMSTot>
                  <vNF>1500.75</vNF>
                </ICMSTot>
              </total>
            </infNFe>
          </NFe>
        </nfeProc>
        """;

    [Fact]
    public async Task ExecuteAsync_ShouldProcessValidXml()
    {
        var repository = new FakeFiscalDocumentRepository();
        var parser = new FiscalDocumentXmlParser();
        var service = new ProcessFiscalDocumentService(
            repository,
            parser);

        var document = new FiscalDocument(
            "empresa-demo",
            "NFE-PROCESSAMENTO",
            xmlContent: ValidXml);

        await repository.InsertAsync(document);

        await service.ExecuteAsync(
            new ProcessFiscalDocumentCommand(
                document.Id,
                document.TenantId));

        Assert.Equal(
            DocumentProcessingStatus.Processed,
            document.Status);
        Assert.NotNull(document.ProcessedAtUtc);
        Assert.Null(document.FailureReason);

        Assert.NotNull(document.FiscalData);
        Assert.Equal(
            "41260612345678000195550010000012341000012345",
            document.FiscalData.AccessKey);
        Assert.Equal(
            "12345678000195",
            document.FiscalData.IssuerDocument);
        Assert.Equal(
            "Empresa Emitente Ltda",
            document.FiscalData.IssuerName);
        Assert.Equal(
            "12345678901",
            document.FiscalData.RecipientDocument);
        Assert.Equal(
            "Cliente Destinatário",
            document.FiscalData.RecipientName);
        Assert.Equal(
            1500.75M,
            document.FiscalData.TotalValue);
        Assert.Equal(
            TimeSpan.FromHours(-3),
            document.FiscalData.IssuedAt.Offset);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMarkAsFailed_WhenXmlIsInvalid()
    {
        var repository = new FakeFiscalDocumentRepository();
        var parser = new FiscalDocumentXmlParser();
        var service = new ProcessFiscalDocumentService(
            repository,
            parser);

        var document = new FiscalDocument(
            "empresa-demo",
            "NFE-INVALIDA",
            xmlContent: "<xml-invalido>");

        await repository.InsertAsync(document);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => service.ExecuteAsync(
                new ProcessFiscalDocumentCommand(
                    document.Id,
                    document.TenantId)));

        Assert.Equal(
            DocumentProcessingStatus.Failed,
            document.Status);
        Assert.Equal(
            "O XML do documento fiscal é inválido.",
            document.FailureReason);
        Assert.Null(document.ProcessedAtUtc);
        Assert.Null(document.FiscalData);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldResume_WhenAlreadyProcessing()
    {
        var repository = new FakeFiscalDocumentRepository();
        var parser = new FiscalDocumentXmlParser();
        var service = new ProcessFiscalDocumentService(
            repository,
            parser);

        var document = new FiscalDocument(
            "empresa-demo",
            "NFE-RETOMADA",
            xmlContent: ValidXml);

        document.MarkAsProcessing();
        await repository.InsertAsync(document);

        await service.ExecuteAsync(
            new ProcessFiscalDocumentCommand(
                document.Id,
                document.TenantId));

        Assert.Equal(
            DocumentProcessingStatus.Processed,
            document.Status);
        Assert.NotNull(document.ProcessedAtUtc);
        Assert.Null(document.FailureReason);
        Assert.NotNull(document.FiscalData);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldParseOnlyOnce_WhenCalledTwice()
    {
        var repository = new FakeFiscalDocumentRepository();
        var parser = new CountingXmlParser();
        var service = new ProcessFiscalDocumentService(
            repository,
            parser);

        var document = new FiscalDocument(
            "empresa-demo",
            "NFE-REENTREGA",
            xmlContent: ValidXml);

        await repository.InsertAsync(document);

        var command = new ProcessFiscalDocumentCommand(
            document.Id,
            document.TenantId);

        await service.ExecuteAsync(command);

        var firstProcessedAtUtc = document.ProcessedAtUtc;
        var firstFiscalData = document.FiscalData;

        await service.ExecuteAsync(command);

        Assert.Equal(1, parser.CallCount);
        Assert.Equal(
            DocumentProcessingStatus.Processed,
            document.Status);
        Assert.Equal(
            firstProcessedAtUtc,
            document.ProcessedAtUtc);
        Assert.Same(firstFiscalData, document.FiscalData);
    }

    private sealed class CountingXmlParser :
        IFiscalDocumentXmlParser
    {
        private readonly IFiscalDocumentXmlParser _inner =
            new FiscalDocumentXmlParser();

        public int CallCount { get; private set; }

        public FiscalDocumentXmlData Parse(string xml)
        {
            CallCount++;
            return _inner.Parse(xml);
        }
    }
}
