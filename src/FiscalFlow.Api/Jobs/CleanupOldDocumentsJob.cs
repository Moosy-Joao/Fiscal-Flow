using FiscalFlow.Api.Configuration;
using FiscalFlow.Application.Documents;
using Hangfire;

namespace FiscalFlow.Api.Jobs;

public sealed class CleanupOldDocumentsJob
{
    private readonly CleanupOldDocumentsService _service;
    private readonly BackgroundJobsOptions _options;
    private readonly ILogger<CleanupOldDocumentsJob> _logger;

    public CleanupOldDocumentsJob(
        CleanupOldDocumentsService service,
        BackgroundJobsOptions options,
        ILogger<CleanupOldDocumentsJob> logger)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _service = service;
        _options = options;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(timeoutInSeconds: 900)]
    public async Task ExecuteAsync()
    {
        var deletedCount = await _service.ExecuteAsync(
            new CleanupOldDocumentsCommand(
                _options.DocumentRetentionDays,
                _options.CleanupBatchSize,
                DateTimeOffset.UtcNow));

        _logger.LogInformation(
            "Limpeza de documentos concluída. Removidos: {DeletedCount}.",
            deletedCount);
    }
}
