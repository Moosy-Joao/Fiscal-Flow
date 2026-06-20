using FiscalFlow.Application.Documents.Xml;

namespace FiscalFlow.UnitTests.Documents;

public sealed class FiscalDocumentXmlParserTests
{
    [Fact]
    public void Parse_ShouldExtractFiscalData()
    {
        const string xml =
            """
            <?xml version="1.0" encoding="utf-8"?>
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

        var parser =
            new FiscalDocumentXmlParser();

        var result = parser.Parse(xml);

        Assert.Equal(
            "41260612345678000195550010000012341000012345",
            result.AccessKey);

        Assert.Equal(
            "12345678000195",
            result.IssuerDocument);

        Assert.Equal(
            "Empresa Emitente Ltda",
            result.IssuerName);

        Assert.Equal(
            "12345678901",
            result.RecipientDocument);

        Assert.Equal(
            "Cliente Destinatário",
            result.RecipientName);

        Assert.Equal(
            1500.75m,
            result.TotalValue);

        Assert.Equal(
            new DateTimeOffset(
                2026,
                6,
                20,
                10,
                30,
                0,
                TimeSpan.FromHours(-3)),
            result.IssuedAt);
    }

    [Fact]
    public void Parse_ShouldThrow_WhenXmlIsInvalid()
    {
        var parser =
            new FiscalDocumentXmlParser();

        var action = () =>
            parser.Parse("<xml-invalido>");

        var exception =
            Assert.Throws<InvalidDataException>(
                action);

        Assert.Equal(
            "O XML do documento fiscal é inválido.",
            exception.Message);
    }
}