namespace FiscalFlow.Application.Documents;

public interface IBackgroundJobScheduler
{
    void EnqueueProcessing(string tenantId, string documentId);
}
