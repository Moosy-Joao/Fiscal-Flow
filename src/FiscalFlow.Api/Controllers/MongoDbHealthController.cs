using FiscalFlow.Infrastructure.MongoDb;
using Microsoft.AspNetCore.Mvc;

namespace FiscalFlow.Api.Controllers;

[ApiController]
[Route("api/health/mongodb")]
public sealed class MongoDbHealthController : ControllerBase
{
    private readonly MongoDbContext _mongoDbContext;
    private readonly ILogger<MongoDbHealthController> _logger;

    public MongoDbHealthController(
        MongoDbContext mongoDbContext,
        ILogger<MongoDbHealthController> logger)
    {
        _mongoDbContext = mongoDbContext;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        try
        {
            await _mongoDbContext.PingAsync(
                cancellationToken);

            return Ok(new
            {
                service = "MongoDB",
                status = "Healthy",
                database = _mongoDbContext.DatabaseName
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Não foi possível conectar ao MongoDB.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    service = "MongoDB",
                    status = "Unhealthy"
                });
        }
    }
}