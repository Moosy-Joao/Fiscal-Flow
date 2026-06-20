using System.Text;
using FiscalFlow.Application.Documents.Xml;

namespace FiscalFlow.UnitTests.Documents;

public sealed class FiscalDocumentXmlFileReaderTests
{
    private const long MaximumSizeBytes =
        2 * 1024 * 1024;

    [Fact]
    public async Task ReadAsync_ShouldReturnValidatedXml()
    {
        var reader = CreateReader();
        await using var stream = CreateStream(ValidXml);

        var result = await reader.ReadAsync(
            "nota-fiscal.xml",
            stream.Length,
            stream,
            MaximumSizeBytes);

        Assert.Contains("infNFe", result);
        Assert.Contains(
            "41260612345678000195550010000012341000012345",
            result);
    }

    [Fact]
    public async Task ReadAsync_ShouldRejectFileWithoutXmlExtension()
    {
        var reader = CreateReader();
        await using var stream = CreateStream(ValidXml);

        var exception =
            await Assert.ThrowsAsync<
                FiscalDocumentUploadValidationException>(
                () => reader.ReadAsync(
                    "nota-fiscal.txt",
                    stream.Length,
                    stream,
                    MaximumSizeBytes));

        Assert.Equal(
            "Somente arquivos com extensão .xml são permitidos.",
            exception.Message);
    }

    [Fact]
    public async Task ReadAsync_ShouldRejectEmptyFile()
    {
        var reader = CreateReader();
        await using var stream = new MemoryStream();

        var exception =
            await Assert.ThrowsAsync<
                FiscalDocumentUploadValidationException>(
                () => reader.ReadAsync(
                    "nota-fiscal.xml",
                    declaredLength: 0,
                    stream,
                    MaximumSizeBytes));

        Assert.Equal(
            "O arquivo XML está vazio.",
            exception.Message);
    }

    [Fact]
    public async Task ReadAsync_ShouldRejectDeclaredLengthAboveLimit()
    {
        var reader = CreateReader();
        await using var stream = CreateStream(ValidXml);

        var exception =
            await Assert.ThrowsAsync<
                FiscalDocumentUploadTooLargeException>(
                () => reader.ReadAsync(
                    "nota-fiscal.xml",
                    MaximumSizeBytes + 1,
                    stream,
                    MaximumSizeBytes));

        Assert.Equal(
            MaximumSizeBytes,
            exception.MaximumSizeBytes);
    }

    [Fact]
    public async Task ReadAsync_ShouldRejectActualContentAboveLimit()
    {
        var reader = CreateReader();
        await using var stream = CreateStream(
            new string('x', 20));

        await Assert.ThrowsAsync<
            FiscalDocumentUploadTooLargeException>(
            () => reader.ReadAsync(
                "nota-fiscal.xml",
                declaredLength: 1,
                stream,
                maximumSizeBytes: 10));
    }

    [Fact]
    public async Task ReadAsync_ShouldRejectMalformedXml()
    {
        var reader = CreateReader();
        await using var stream = CreateStream(
            "<nfeProc>");

        var exception =
            await Assert.ThrowsAsync<
                FiscalDocumentUploadValidationException>(
                () => reader.ReadAsync(
                    "nota-fiscal.xml",
                    stream.Length,
                    stream,
                    MaximumSizeBytes));

        Assert.Equal(
            "O arquivo enviado não contém um XML válido.",
            exception.Message);
    }

    [Fact]
    public async Task ReadAsync_ShouldRejectDocumentTypeDefinition()
    {
        const string xmlWithDtd =
            """
            <!DOCTYPE nfeProc [
              <!ENTITY external SYSTEM "file:///etc/passwd">
            ]>
            <nfeProc>&external;</nfeProc>
            """;

        var reader = CreateReader();
        await using var stream = CreateStream(xmlWithDtd);

        var exception =
            await Assert.ThrowsAsync<
                FiscalDocumentUploadValidationException>(
                () => reader.ReadAsync(
                    "nota-fiscal.xml",
                    stream.Length,
                    stream,
                    MaximumSizeBytes));

        Assert.Equal(
            "O arquivo enviado não contém um XML válido.",
            exception.Message);
    }

    [Fact]
    public async Task ReadAsync_ShouldRejectXmlWithoutFiscalData()
    {
        const string nonFiscalXml =
            "<documento><valor>100</valor></documento>";

        var reader = CreateReader();
        await using var stream = CreateStream(nonFiscalXml);

        var exception =
            await Assert.ThrowsAsync<
                FiscalDocumentUploadValidationException>(
                () => reader.ReadAsync(
                    "documento.xml",
                    stream.Length,
                    stream,
                    MaximumSizeBytes));

        Assert.Equal(
            "O elemento infNFe não foi encontrado.",
            exception.Message);
    }

    private static FiscalDocumentXmlFileReader CreateReader()
    {
        return new FiscalDocumentXmlFileReader(
            new FiscalDocumentXmlParser());
    }

    private static MemoryStream CreateStream(
        string content)
    {
        return new MemoryStream(
            Encoding.UTF8.GetBytes(content));
    }

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
}
