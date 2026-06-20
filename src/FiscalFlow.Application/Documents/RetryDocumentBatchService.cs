namespace FiscalFlow.Application.Documents;

public sealed class RetryDocumentBatchService
{
    private readonly IFiscalDocumentRepository _repository;
    private readonly ProcessFiscalDocumentService _processService;

    public RetryDocumentBatchService(
        IFiscalDocumentRepository repository,
        ProcessFiscalDocumentService processService)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(processService);

        _repository = repository;
        _processService = processService;
    }

    public async Task<RetryBatchResult> ExecuteAsync(
        FailedDocumentBatchCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.MaximumAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command),
                "O limite de tentativas deve ser maior que zero.");
        }

        if (command.BatchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command),
                "O tamanho do lote deve ser maior que zero.");
        }

        var documents =
            await _repository.ClaimFailedForReprocessingAsync(
                command.MaximumAttempts,
                command.BatchSize,
                cancellationToken);

        var processedCount = 0;
        var failedCount = 0;

        foreach (var document in documents)
        {
            try
            {
                await _processService.ExecuteAsync(
                    new ProcessFiscalDocumentCommand(
                        document.Id,
                        document.TenantId),
                    cancellationToken);

                processedCount++;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                failedCount++;
            }
        }

        return new RetryBatchResult(
            documents.Count,
            processedCount,
            failedCount);
    }
}
