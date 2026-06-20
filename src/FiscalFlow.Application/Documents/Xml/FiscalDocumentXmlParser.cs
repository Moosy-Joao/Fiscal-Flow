using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace FiscalFlow.Application.Documents.Xml;

public sealed class FiscalDocumentXmlParser :
    IFiscalDocumentXmlParser
{
    public FiscalDocumentXmlData Parse(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        var document = LoadDocument(xml);

        var infNFe = document
            .Descendants()
            .FirstOrDefault(element =>
                element.Name.LocalName == "infNFe")
            ?? throw new InvalidDataException(
                "O elemento infNFe não foi encontrado.");

        var ide = GetRequiredElement(
            infNFe,
            "ide");

        var emit = GetRequiredElement(
            infNFe,
            "emit");

        var dest = GetRequiredElement(
            infNFe,
            "dest");

        var total = GetRequiredElement(
            infNFe,
            "total");

        var icmsTotal = GetRequiredElement(
            total,
            "ICMSTot");

        var accessKey =
            ReadAccessKey(infNFe);

        var issuerDocument =
            ReadPersonDocument(emit);

        var issuerName =
            GetRequiredValue(
                emit,
                "xNome");

        var recipientDocument =
            ReadPersonDocument(dest);

        var recipientName =
            GetRequiredValue(
                dest,
                "xNome");

        var totalValue =
            ReadDecimal(
                GetRequiredValue(
                    icmsTotal,
                    "vNF"),
                "vNF");

        var issuedAt =
            ReadIssuedAt(ide);

        return new FiscalDocumentXmlData(
            accessKey,
            issuerDocument,
            issuerName,
            recipientDocument,
            recipientName,
            totalValue,
            issuedAt);
    }

    private static XDocument LoadDocument(
        string xml)
    {
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing =
                    DtdProcessing.Prohibit,

                XmlResolver = null,

                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using var stringReader =
                new StringReader(xml);

            using var xmlReader =
                XmlReader.Create(
                    stringReader,
                    settings);

            return XDocument.Load(
                xmlReader,
                LoadOptions.None);
        }
        catch (XmlException exception)
        {
            throw new InvalidDataException(
                "O XML do documento fiscal é inválido.",
                exception);
        }
    }

    private static string ReadAccessKey(
        XElement infNFe)
    {
        var identifier =
            infNFe.Attribute("Id")?.Value.Trim();

        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new InvalidDataException(
                "O atributo Id de infNFe não foi informado.");
        }

        var accessKey =
            identifier.StartsWith(
                "NFe",
                StringComparison.OrdinalIgnoreCase)
                ? identifier[3..]
                : identifier;

        if (accessKey.Length != 44
            || accessKey.Any(character =>
                !char.IsDigit(character)))
        {
            throw new InvalidDataException(
                "A chave de acesso da NF-e deve conter 44 números.");
        }

        return accessKey;
    }

    private static string ReadPersonDocument(
        XElement personElement)
    {
        var documentElement =
            personElement
                .Elements()
                .FirstOrDefault(element =>
                    element.Name.LocalName
                        is "CNPJ" or "CPF");

        if (documentElement is null
            || string.IsNullOrWhiteSpace(
                documentElement.Value))
        {
            throw new InvalidDataException(
                $"CNPJ ou CPF não encontrado em {personElement.Name.LocalName}.");
        }

        return documentElement.Value.Trim();
    }

    private static DateTimeOffset ReadIssuedAt(
        XElement ide)
    {
        var issuedAtText =
            ide.Elements()
                .FirstOrDefault(element =>
                    element.Name.LocalName == "dhEmi")
                ?.Value
            ?? ide.Elements()
                .FirstOrDefault(element =>
                    element.Name.LocalName == "dEmi")
                ?.Value;

        if (string.IsNullOrWhiteSpace(issuedAtText)
            || !DateTimeOffset.TryParse(
                issuedAtText,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var issuedAt))
        {
            throw new InvalidDataException(
                "A data de emissão da NF-e é inválida.");
        }

        return issuedAt;
    }

    private static decimal ReadDecimal(
        string value,
        string fieldName)
    {
        if (!decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var result))
        {
            throw new InvalidDataException(
                $"O campo {fieldName} possui um valor inválido.");
        }

        return result;
    }

    private static XElement GetRequiredElement(
        XContainer container,
        string localName)
    {
        return container
            .Elements()
            .FirstOrDefault(element =>
                element.Name.LocalName == localName)
            ?? throw new InvalidDataException(
                $"O elemento {localName} não foi encontrado.");
    }

    private static string GetRequiredValue(
        XContainer container,
        string localName)
    {
        var value =
            GetRequiredElement(
                container,
                localName)
            .Value
            .Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException(
                $"O elemento {localName} está vazio.");
        }

        return value;
    }
}