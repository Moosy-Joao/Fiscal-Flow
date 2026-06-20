using FiscalFlow.Api.Contracts;
using FiscalFlow.Application.Documents;
using FiscalFlow.Domain.Documents;
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

    private readonly UpdateFiscalDocumentStatusService
    _updateStatusService;

    public FiscalDocumentsController(
    CreateFiscalDocumentService createService,
    GetFiscalDocumentByIdService getByIdService,
    UpdateFiscalDocumentStatusService updateStatusService)
    {
        _createService = createService;
        _getByIdService = getByIdService;
        _updateStatusService = updateStatusService;
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

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(
    typeof(FiscalDocumentDetails),
    StatusCodes.Status200OK)]
    [ProducesResponseType(
    StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
    StatusCodes.Status404NotFound)]
    [ProducesResponseType(
    StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStatus(
    Guid id,
    UpdateFiscalDocumentStatusRequest request,
    CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<DocumentProcessingStatus>(
            request.Status,
            ignoreCase: true,
            out var status)
            || !Enum.IsDefined(
                typeof(DocumentProcessingStatus),
                status))
        {
            ModelState.AddModelError(
                nameof(request.Status),
                "Status inválido. Use Processing, Processed ou Failed.");

            return ValidationProblem(ModelState);
        }

        var command =
            new UpdateFiscalDocumentStatusCommand(
                id,
                status,
                request.FailureReason);

        try
        {
            var result =
                await _updateStatusService.ExecuteAsync(
                    command,
                    cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(
                "request",
                exception.Message);

            return ValidationProblem(ModelState);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Transição de status inválida",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}