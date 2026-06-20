using FiscalFlow.Application.Documents.Xml;

namespace FiscalFlow.Application.Documents;

public sealed class ProcessFiscalDocumentService
{
    private readonly IFiscalDocumentRepository
        _repository;

    private readonly IFiscalDocumentXmlParser
        _xmlParser;

    public ProcessFiscalDocumentService(
        IFiscalDocumentRepository repository,
        IFiscalDocumentXmlParser xmlParser)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(xmlParser);

        _repository = repository;
        _xmlParser = xmlParser;
    }

    public async Task ExecuteAsync(
        ProcessFiscalDocumentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.DocumentId == Guid.Empty)
        {
            throw new ArgumentException(
                "O ID do documento não pode ser vazio.",
                nameof(command));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            command.TenantId);

        var tenantId = command.TenantId.Trim();

        var existingDocument =
            await _repository.FindDomainByIdAsync(
                command.DocumentId,
                tenantId,
                cancellationToken);

        if (existingDocument is null)
        {
            throw new KeyNotFoundException(
                $"O documento fiscal {command.DocumentId} não foi encontrado.");
        }

        var document =
            await _repository.TryStartProcessingAsync(
                command.DocumentId,
                tenantId,
                cancellationToken);

        if (document is null)
        {
            return;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(
                    document.XmlContent))
            {
                throw new InvalidDataException(
                    "O documento fiscal não possui conteúdo XML.");
            }

            var parsedData = _xmlParser.Parse(
                document.XmlContent);

            document.CompleteProcessing(
                FiscalDocumentDataMapper.Map(parsedData));

            await _repository.UpdateAsync(
                document,
                cancellationToken);
        }
        catch (Exception exception)
            when (exception is not
                OperationCanceledException)
        {
            document.MarkAsFailed(
                exception.Message);

            await _repository.UpdateAsync(
                document,
                cancellationToken);

            throw;
        }
    }
}
