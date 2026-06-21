namespace FiscalFlow.Application.Documents;

public sealed class DetectTimedOutProcessingService
{
    private readonly IProcessingTimeoutRepository _repository;

    public DetectTimedOutProcessingService(
        IProcessingTimeoutRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public Task<int> ExecuteAsync(
        DetectTimedOutProcessingCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.ProcessingTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command),
                "O tempo limite de processamento deve ser maior que zero.");
        }

        if (command.BatchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command),
                "O tamanho do lote deve ser maior que zero.");
        }

        var startedBeforeUtc =
            command.UtcNow.Subtract(command.ProcessingTimeout);

        return _repository.MarkTimedOutProcessingAsFailedAsync(
            startedBeforeUtc,
            command.BatchSize,
            cancellationToken);
    }
}
