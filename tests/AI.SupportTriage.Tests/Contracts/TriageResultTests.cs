using AI.SupportTriage.Api.Contracts;
using AI.SupportTriage.Api.Domain;

namespace AI.SupportTriage.Tests.Contracts;

public sealed class TriageResultTests
{
    [Fact]
    public void Result_UsesDomainTypes()
    {
        var result = new TriageResult
        {
            Category = SupportCategory.Payments,
            Priority = SupportPriority.High,
            Sentiment = SupportSentiment.Frustrated,
            Summary = "Payment was declined.",
            SuggestedTeam = "Payments Operations"
        };

        Assert.IsType<SupportCategory>(result.Category);
        Assert.IsType<SupportPriority>(result.Priority);
        Assert.IsType<SupportSentiment>(result.Sentiment);
    }
}
