using FiscalFlow.Api.Contracts;
using FiscalFlow.Application.Documents;
using Microsoft.AspNetCore.Mvc;

namespace FiscalFlow.Api.Controllers;

[ApiController]
[Route("api/fiscal-documents")]
public sealed class FiscalDocumentsController
    : ControllerBase
{
    private readonly CreateFiscalDocumentService
        _createService;

    private readonly GetFiscalDocumentByIdService
        _getByIdService;

    public FiscalDocumentsController(
        CreateFiscalDocumentService createService,
        GetFiscalDocumentByIdService getByIdService)
    {
        _createService = createService;
        _getByIdService = getByIdService;
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

        var result = await _createService.ExecuteAsync(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(FiscalDocumentDetails),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getByIdService.ExecuteAsync(
            id,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}