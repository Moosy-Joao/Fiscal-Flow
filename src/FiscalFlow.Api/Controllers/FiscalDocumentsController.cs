using FiscalFlow.Api.Contracts;
using FiscalFlow.Application.Documents;
using Microsoft.AspNetCore.Mvc;

namespace FiscalFlow.Api.Controllers;

[ApiController]
[Route("api/fiscal-documents")]
public sealed class FiscalDocumentsController
    : ControllerBase
{
    private readonly CreateFiscalDocumentService _service;

    public FiscalDocumentsController(
        CreateFiscalDocumentService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CreateFiscalDocumentResult),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        CreateFiscalDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateFiscalDocumentCommand(
            request.TenantId,
            request.ExternalDocumentId);

        var result = await _service.ExecuteAsync(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}