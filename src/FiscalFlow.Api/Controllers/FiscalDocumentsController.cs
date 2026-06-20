using FiscalFlow.Api.Configuration;
using FiscalFlow.Api.Contracts;
using FiscalFlow.Api.Tenancy;
using FiscalFlow.Application.Documents;
using FiscalFlow.Application.Documents.Xml;
using FiscalFlow.Domain.Documents;
using Microsoft.AspNetCore.Mvc;

namespace FiscalFlow.Api.Controllers;

[ApiController]
[Route("api/fiscal-documents")]
public sealed class FiscalDocumentsController :
    ControllerBase
{
    private readonly CreateFiscalDocumentService
        _createService;

    private readonly GetFiscalDocumentByIdService
        _getByIdService;

    private readonly UpdateFiscalDocumentStatusService
        _updateStatusService;

    private readonly ListFiscalDocumentsService
        _listService;

    private readonly FiscalDocumentXmlFileReader
        _xmlFileReader;

    private readonly FiscalDocumentUploadOptions
        _uploadOptions;

    private readonly TenantContext
        _tenantContext;

    public FiscalDocumentsController(
        CreateFiscalDocumentService createService,
        GetFiscalDocumentByIdService getByIdService,
        UpdateFiscalDocumentStatusService updateStatusService,
        ListFiscalDocumentsService listService,
        FiscalDocumentXmlFileReader xmlFileReader,
        FiscalDocumentUploadOptions uploadOptions,
        TenantContext tenantContext)
    {
        _createService = createService;
        _getByIdService = getByIdService;
        _updateStatusService = updateStatusService;
        _listService = listService;
        _xmlFileReader = xmlFileReader;
        _uploadOptions = uploadOptions;
        _tenantContext = tenantContext;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CreateFiscalDocumentResult),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(CreateFiscalDocumentResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public Task<IActionResult> Create(
        CreateFiscalDocumentRequest request,
        CancellationToken cancellationToken)
    {
        return CreateFromContentAsync(
            request.ExternalDocumentId,
            request.XmlContent,
            cancellationToken);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(
        typeof(CreateFiscalDocumentResult),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(CreateFiscalDocumentResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> Upload(
        [FromForm] UploadFiscalDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream =
                request.File.OpenReadStream();

            var xmlContent =
                await _xmlFileReader.ReadAsync(
                    request.File.FileName,
                    request.File.Length,
                    stream,
                    _uploadOptions.MaxFileSizeBytes,
                    cancellationToken);

            return await CreateFromContentAsync(
                request.ExternalDocumentId,
                xmlContent,
                cancellationToken);
        }
        catch (FiscalDocumentUploadTooLargeException exception)
        {
            return StatusCode(
                StatusCodes.Status413PayloadTooLarge,
                new ProblemDetails
                {
                    Title = "Arquivo XML muito grande",
                    Detail = exception.Message,
                    Status =
                        StatusCodes.Status413PayloadTooLarge
                });
        }
        catch (FiscalDocumentUploadValidationException exception)
        {
            ModelState.AddModelError(
                nameof(request.File),
                exception.Message);

            return ValidationProblem(ModelState);
        }
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<FiscalDocumentDetails>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] ListFiscalDocumentsRequest request,
        CancellationToken cancellationToken)
    {
        DocumentProcessingStatus? status = null;

        if (!string.IsNullOrWhiteSpace(
                request.Status))
        {
            if (!Enum.TryParse<DocumentProcessingStatus>(
                    request.Status,
                    ignoreCase: true,
                    out var parsedStatus)
                || !Enum.IsDefined(
                    typeof(DocumentProcessingStatus),
                    parsedStatus))
            {
                ModelState.AddModelError(
                    nameof(request.Status),
                    "Status inválido.");

                return ValidationProblem(ModelState);
            }

            status = parsedStatus;
        }

        var query = new ListFiscalDocumentsQuery(
            _tenantContext.TenantId,
            status,
            request.Page,
            request.PageSize);

        var result = await _listService.ExecuteAsync(
            query,
            cancellationToken);

        return Ok(result);
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
            _tenantContext.TenantId,
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
                _tenantContext.TenantId,
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
                Title =
                    "Transição de status inválida",
                Detail = exception.Message,
                Status =
                    StatusCodes.Status409Conflict
            });
        }
    }

    private async Task<IActionResult> CreateFromContentAsync(
        string externalDocumentId,
        string xmlContent,
        CancellationToken cancellationToken)
    {
        var command = new CreateFiscalDocumentCommand(
            _tenantContext.TenantId,
            externalDocumentId,
            xmlContent);

        var result = await _createService.ExecuteAsync(
            command,
            cancellationToken);

        if (!result.WasCreated)
        {
            return Ok(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }
}
