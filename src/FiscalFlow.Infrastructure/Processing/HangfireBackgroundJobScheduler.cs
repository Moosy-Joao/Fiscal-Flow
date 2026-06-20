using FiscalFlow.Application.Documents;
using Hangfire;

namespace FiscalFlow.Infrastructure.Processing;

public sealed class HangfireBackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly IBackgroundJobClient _jobs;

    public HangfireBackgroundJobScheduler(IBackgroundJobClient jobs)
    {
        _jobs = jobs;
    }

    public void EnqueueProcessing(string tenantId, string documentId)
    {
        _jobs.Enqueue<FiscalDocumentProcessingJob>(job => job.ProcessAsync(tenantId, documentId));
    }
}
