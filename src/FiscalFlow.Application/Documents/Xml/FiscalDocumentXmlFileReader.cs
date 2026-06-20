using System.Xml;
using System.Xml.Linq;

namespace FiscalFlow.Application.Documents.Xml;

public sealed class FiscalDocumentXmlFileReader
{
    private const int BufferSize = 81920;

    private readonly IFiscalDocumentXmlParser _xmlParser;

    public FiscalDocumentXmlFileReader(
        IFiscalDocumentXmlParser xmlParser)
    {
        ArgumentNullException.ThrowIfNull(xmlParser);

        _xmlParser = xmlParser;
    }

    public async Task<string> ReadAsync(
        string fileName,
        long declaredLength,
        Stream content,
        long maximumSizeBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new FiscalDocumentUploadValidationException(
                "O nome do arquivo XML não foi informado.");
        }

        if (!string.Equals(
                Path.GetExtension(fileName),
                ".xml",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new FiscalDocumentUploadValidationException(
                "Somente arquivos com extensão .xml são permitidos.");
        }

        if (maximumSizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumSizeBytes),
                "O limite do arquivo deve ser maior que zero.");
        }

        if (declaredLength <= 0)
        {
            throw new FiscalDocumentUploadValidationException(
                "O arquivo XML está vazio.");
        }

        if (declaredLength > maximumSizeBytes)
        {
            throw new FiscalDocumentUploadTooLargeException(
                maximumSizeBytes);
        }

        if (!content.CanRead)
        {
            throw new FiscalDocumentUploadValidationException(
                "Não foi possível ler o arquivo XML.");
        }

        await using var buffer = new MemoryStream();
        var readBuffer = new byte[BufferSize];
        long totalBytesRead = 0;

        while (true)
        {
            var bytesRead = await content.ReadAsync(
                readBuffer.AsMemory(0, readBuffer.Length),
                cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            totalBytesRead += bytesRead;

            if (totalBytesRead > maximumSizeBytes)
            {
                throw new FiscalDocumentUploadTooLargeException(
                    maximumSizeBytes);
            }

            await buffer.WriteAsync(
                readBuffer.AsMemory(0, bytesRead),
                cancellationToken);
        }

        if (totalBytesRead == 0)
        {
            throw new FiscalDocumentUploadValidationException(
                "O arquivo XML está vazio.");
        }

        buffer.Position = 0;

        string xmlContent;

        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                IgnoreComments = true,
                IgnoreWhitespace = false,
                CloseInput = false
            };

            using var xmlReader = XmlReader.Create(
                buffer,
                settings);

            var document = XDocument.Load(
                xmlReader,
                LoadOptions.PreserveWhitespace);

            xmlContent = document.ToString(
                SaveOptions.DisableFormatting);
        }
        catch (XmlException exception)
        {
            throw new FiscalDocumentUploadValidationException(
                "O arquivo enviado não contém um XML válido.",
                exception);
        }

        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            throw new FiscalDocumentUploadValidationException(
                "O arquivo XML está vazio.");
        }

        try
        {
            _xmlParser.Parse(xmlContent);
        }
        catch (InvalidDataException exception)
        {
            throw new FiscalDocumentUploadValidationException(
                exception.Message,
                exception);
        }

        return xmlContent;
    }
}
