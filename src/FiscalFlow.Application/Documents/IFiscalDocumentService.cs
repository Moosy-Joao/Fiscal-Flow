using System.Threading;
using System.Threading.Tasks;

namespace FiscalFlow.Application.Documents;

public interface IFiscalDocumentService
{
    Task<SubmitFiscalDocumentResult> SubmitAsync(string tenantId, string idempotencyKey, SubmitFiscalDocumentRequest request, CancellationToken cancellationToken);
}
