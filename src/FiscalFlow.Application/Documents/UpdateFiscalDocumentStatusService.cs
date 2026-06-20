using FiscalFlow.Domain.Documents;

namespace FiscalFlow.Application.Documents;

public sealed class UpdateFiscalDocumentStatusService
{
    private readonly IFiscalDocumentRepository _repository;

    public UpdateFiscalDocumentStatusService(
        IFiscalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<FiscalDocumentDetails?> ExecuteAsync(
        UpdateFiscalDocumentStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Id == Guid.Empty)
        {
            throw new ArgumentException(
                "O ID do documento não pode ser vazio.",
                nameof(command));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            command.TenantId,
            nameof(command.TenantId));

        var tenantId = command.TenantId.Trim();

        var document =
            await _repository.FindDomainByIdAsync(
                command.Id,
                tenantId,
                cancellationToken);

        if (document is null)
        {
            return null;
        }

        switch (command.Status)
        {
            case DocumentProcessingStatus.Processing:
                document.MarkAsProcessing();
                break;

            case DocumentProcessingStatus.Processed:
                document.MarkAsProcessed();
                break;

            case DocumentProcessingStatus.Failed:
                if (string.IsNullOrWhiteSpace(
                        command.FailureReason))
                {
                    throw new ArgumentException(
                        "O motivo da falha é obrigatório.");
                }

                document.MarkAsFailed(
                    command.FailureReason);

                break;

            case DocumentProcessingStatus.Received:
                throw new InvalidOperationException(
                    "Não é permitido retornar um documento ao status Received.");

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(command),
                    "Status desconhecido.");
        }

        await _repository.UpdateAsync(
            document,
            cancellationToken);

        return new FiscalDocumentDetails(
            document.Id,
            document.TenantId,
            document.ExternalDocumentId,
            document.Status.ToString(),
            document.ReceivedAtUtc,
            document.ProcessedAtUtc,
            document.FailureReason);
    }
}