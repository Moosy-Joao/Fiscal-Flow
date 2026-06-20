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
        var repository =
            new FakeFiscalDocumentRepository();

        var parser =
            new FiscalDocumentXmlParser();

        var service =
            new ProcessFiscalDocumentService(
                repository,
                parser);

        var document =
            new FiscalDocument(
                "empresa-demo",
                "NFE-PROCESSAMENTO",
                xmlContent: ValidXml);

        await repository.InsertAsync(document);

        var command =
            new ProcessFiscalDocumentCommand(
                document.Id,
                document.TenantId);

        await service.ExecuteAsync(command);

        Assert.Equal(
            DocumentProcessingStatus.Processed,
            document.Status);

        Assert.NotNull(
            document.ProcessedAtUtc);

        Assert.Null(
            document.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMarkAsFailed_WhenXmlIsInvalid()
    {
        var repository =
            new FakeFiscalDocumentRepository();

        var parser =
            new FiscalDocumentXmlParser();

        var service =
            new ProcessFiscalDocumentService(
                repository,
                parser);

        var document =
            new FiscalDocument(
                "empresa-demo",
                "NFE-INVALIDA",
                xmlContent: "<xml-invalido>");

        await repository.InsertAsync(document);

        var command =
            new ProcessFiscalDocumentCommand(
                document.Id,
                document.TenantId);

        await Assert.ThrowsAsync<
            InvalidDataException>(
                () => service.ExecuteAsync(command));

        Assert.Equal(
            DocumentProcessingStatus.Failed,
            document.Status);

        Assert.Equal(
            "O XML do documento fiscal é inválido.",
            document.FailureReason);

        Assert.Null(
            document.ProcessedAtUtc);
    }
}