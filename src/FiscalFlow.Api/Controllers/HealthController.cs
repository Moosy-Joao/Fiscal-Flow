using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FiscalFlow.Api.Controllers;

[ApiController]
[AllowAnonymous]
[DisableRateLimiting]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new
        {
            application = "FiscalFlow",
            status = "Healthy",
            checkedAtUtc = DateTimeOffset.UtcNow
        });
    }
}
