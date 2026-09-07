using AI.SupportTriage.Api.Contracts;
using AI.SupportTriage.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AI.SupportTriage.Api.Controllers;

[ApiController]
[Route("api/triage")]
public sealed class TriageController(ITriageService triageService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TriageResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TriageResult>> Analyze(
        TriageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await triageService.AnalyzeAsync(request, cancellationToken);
        return Ok(result);
    }
}
