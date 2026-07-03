using Microsoft.AspNetCore.Mvc;

namespace WhoIsInV2.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "ok", timeUtc = DateTime.UtcNow });
    }
}
