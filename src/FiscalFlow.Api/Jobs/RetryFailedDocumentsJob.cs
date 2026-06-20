using FiscalFlow.Api.Configuration;
using FiscalFlow.Application.Documents;
using Hangfire;

namespace FiscalFlow.Api.Jobs;

public sealed class RetryFailedDocumentsJob
{
    private readonly RetryDocumentBatchService _service;
    private readonly BackgroundJobsOptions _options;
    private readonly ILogger<RetryFailedDocumentsJob> _logger;

    public RetryFailedDocumentsJob(
        RetryDocumentBatchService service,
        BackgroundJobsOptions options,
        ILogger<RetryFailedDocumentsJob> logger)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _service = service;
        _options = options;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task ExecuteAsync()
    {
        var result = await _service.ExecuteAsync(
            new FailedDocumentBatchCommand(
                _options.MaximumFailedAttempts,
                _options.FailedBatchSize));

        _logger.LogInformation(
            "Lote de documentos com falha concluído. Capturados: {ClaimedCount}; processados: {ProcessedCount}; falharam: {FailedCount}.",
            result.ClaimedCount,
            result.ProcessedCount,
            result.FailedCount);
    }
}
