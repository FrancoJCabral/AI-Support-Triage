using AI.SupportTriage.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AI.SupportTriage.Api.Infrastructure;

public sealed class TriageExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not TriageProviderException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status502BadGateway;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = "The triage provider is temporarily unavailable.",
                Detail = "The support request could not be analyzed."
            },
            cancellationToken);

        return true;
    }
}
