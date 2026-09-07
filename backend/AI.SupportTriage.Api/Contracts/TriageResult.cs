using AI.SupportTriage.Api.Domain;

namespace AI.SupportTriage.Api.Contracts;

public sealed class TriageResult
{
    public SupportCategory Category { get; init; }

    public SupportPriority Priority { get; init; }

    public SupportSentiment Sentiment { get; init; }

    public string Summary { get; init; } = string.Empty;

    public string SuggestedTeam { get; init; } = string.Empty;
}
