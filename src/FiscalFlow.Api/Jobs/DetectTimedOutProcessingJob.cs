using FiscalFlow.Api.Configuration;
using FiscalFlow.Application.Documents;
using Hangfire;

namespace FiscalFlow.Api.Jobs;

public sealed class DetectTimedOutProcessingJob
{
    private readonly DetectTimedOutProcessingService _service;
    private readonly BackgroundJobsOptions _options;
    private readonly ILogger<DetectTimedOutProcessingJob> _logger;

    public DetectTimedOutProcessingJob(
        DetectTimedOutProcessingService service,
        BackgroundJobsOptions options,
        ILogger<DetectTimedOutProcessingJob> logger)
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
        var recoveredCount = await _service.ExecuteAsync(
            new DetectTimedOutProcessingCommand(
                TimeSpan.FromMinutes(
                    _options.ProcessingTimeoutMinutes),
                _options.TimedOutProcessingBatchSize,
                DateTimeOffset.UtcNow));

        _logger.LogInformation(
            "Detecção de timeout concluída. Documentos marcados como falha: {RecoveredCount}.",
            recoveredCount);
    }
}
