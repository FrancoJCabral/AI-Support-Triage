using Microsoft.AspNetCore.Mvc;

namespace AI.SupportTriage.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> Get()
    {
        return Ok(new { status = "ok" });
    }
}
