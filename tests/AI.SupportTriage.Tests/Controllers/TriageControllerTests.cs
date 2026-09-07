using AI.SupportTriage.Api.Contracts;
using AI.SupportTriage.Api.Controllers;
using AI.SupportTriage.Api.Domain;
using AI.SupportTriage.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AI.SupportTriage.Tests.Controllers;

public sealed class TriageControllerTests
{
    [Fact]
    public async Task Analyze_ValidRequest_ReturnsOkWithCompleteResult()
    {
        var expected = new TriageResult
        {
            Category = SupportCategory.Payments,
            Priority = SupportPriority.High,
            Sentiment = SupportSentiment.Frustrated,
            Summary = "Payment issue",
            SuggestedTeam = "Payments Support"
        };
        var service = new StubTriageService(expected);
        var controller = new TriageController(service);

        var response = await controller.Analyze(
            new TriageRequest { Message = "Payment failed" },
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var result = Assert.IsType<TriageResult>(ok.Value);
        Assert.Equal(200, ok.StatusCode);
        Assert.Equal(SupportCategory.Payments, result.Category);
        Assert.Equal(SupportPriority.High, result.Priority);
        Assert.Equal(SupportSentiment.Frustrated, result.Sentiment);
        Assert.Equal("Payment issue", result.Summary);
        Assert.Equal("Payments Support", result.SuggestedTeam);
    }

    [Fact]
    public async Task Analyze_PropagatesCancellationToken()
    {
        var service = new StubTriageService(new TriageResult());
        var controller = new TriageController(service);
        using var source = new CancellationTokenSource();

        await controller.Analyze(new TriageRequest { Message = "Question" }, source.Token);

        Assert.Equal(source.Token, service.ReceivedCancellationToken);
    }

    private sealed class StubTriageService(TriageResult result) : ITriageService
    {
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<TriageResult> AnalyzeAsync(
            TriageRequest request,
            CancellationToken cancellationToken = default)
        {
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(result);
        }
    }
}
