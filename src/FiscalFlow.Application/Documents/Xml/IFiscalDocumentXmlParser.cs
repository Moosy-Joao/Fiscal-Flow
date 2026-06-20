namespace FiscalFlow.Application.Documents.Xml;

public interface IFiscalDocumentXmlParser
{
    FiscalDocumentXmlData Parse(string xml);
}