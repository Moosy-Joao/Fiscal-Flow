using System.Text.Json;
using FiscalFlow.Api.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace FiscalFlow.IntegrationTests;

public sealed class ApiExceptionHandlerTests
{
    [Theory]
    [InlineData(typeof(ArgumentException), 400)]
    [InlineData(typeof(KeyNotFoundException), 404)]
    [InlineData(typeof(InvalidOperationException), 409)]
    public async Task KnownException_ShouldReturnMappedStatus(
        Type exceptionType,
        int expectedStatus)
    {
        var exception = (Exception)Activator.CreateInstance(
            exceptionType,
            "Mensagem conhecida.")!;

        var (context, problem) = await ExecuteAsync(exception);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.Equal(expectedStatus, problem.Status);
        Assert.Equal(
            "correlation-test",
            problem.Extensions["correlationId"]?.ToString());
    }

    [Fact]
    public async Task UnexpectedException_ShouldHideInternalMessage()
    {
        var (context, problem) = await ExecuteAsync(
            new Exception("detalhe interno sensível"));

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            context.Response.StatusCode);
        Assert.Equal("Erro interno", problem.Title);
        Assert.DoesNotContain(
            "detalhe interno sensível",
            problem.Detail ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<(
        DefaultHttpContext Context,
        ProblemDetails Problem)> ExecuteAsync(
        Exception exception)
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "correlation-test"
        };

        context.Request.Path = "/api/fiscal-documents";
        context.Response.Body = new MemoryStream();

        var handler = new ApiExceptionHandler(
            NullLogger<ApiExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        Assert.True(handled);

        context.Response.Body.Position = 0;

        var problem = await JsonSerializer.DeserializeAsync<
            ProblemDetails>(
                context.Response.Body,
                new JsonSerializerOptions(
                    JsonSerializerDefaults.Web));

        return (
            context,
            Assert.IsType<ProblemDetails>(problem));
    }
}
