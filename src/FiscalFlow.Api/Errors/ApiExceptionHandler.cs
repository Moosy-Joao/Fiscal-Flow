using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FiscalFlow.Api.Errors;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(
        ILogger<ApiExceptionHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        var response = Map(exception);

        if (response.Status >= 500)
        {
            _logger.LogError(
                exception,
                "Falha não tratada durante a requisição.");
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Requisição encerrada com status {StatusCode}.",
                response.Status);
        }

        httpContext.Response.StatusCode = response.Status;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{response.Status}",
                Title = response.Title,
                Detail = response.Detail,
                Status = response.Status,
                Instance = httpContext.Request.Path,
                Extensions =
                {
                    ["correlationId"] =
                        httpContext.TraceIdentifier
                }
            },
            cancellationToken);

        return true;
    }

    private static ErrorResponse Map(Exception exception)
    {
        return exception switch
        {
            ArgumentException or InvalidDataException =>
                new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Requisição inválida",
                    exception.Message),
            KeyNotFoundException =>
                new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Recurso não encontrado",
                    exception.Message),
            InvalidOperationException =>
                new ErrorResponse(
                    StatusCodes.Status409Conflict,
                    "Conflito de operação",
                    exception.Message),
            UnauthorizedAccessException =>
                new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Acesso negado",
                    "A identidade não possui permissão para esta operação."),
            _ =>
                new ErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    "Erro interno",
                    "Ocorreu um erro inesperado ao processar a requisição.")
        };
    }

    private sealed record ErrorResponse(
        int Status,
        string Title,
        string Detail);
}
