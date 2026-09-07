using AI.SupportTriage.Api.Contracts;

namespace AI.SupportTriage.Api.Services;

public interface ITriageService
{
    Task<TriageResult> AnalyzeAsync(
        TriageRequest request,
        CancellationToken cancellationToken = default);
}
