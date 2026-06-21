namespace FiscalFlow.Application.Documents;

public sealed class CleanupOldDocumentsService
{
    private readonly IDocumentCleanupRepository _repository;

    public CleanupOldDocumentsService(
        IDocumentCleanupRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public Task<int> ExecuteAsync(
        CleanupOldDocumentsCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.RetentionDays <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command),
                "O período de retenção deve ser maior que zero.");
        }

        if (command.BatchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command),
                "O tamanho do lote deve ser maior que zero.");
        }

        var receivedBeforeUtc =
            command.UtcNow.AddDays(-command.RetentionDays);

        return _repository.DeleteOldFinalDocumentsAsync(
            receivedBeforeUtc,
            command.BatchSize,
            cancellationToken);
    }
}
