using AI.SupportTriage.Api.Contracts;
using AI.SupportTriage.Api.Domain;
using AI.SupportTriage.Api.Services;

namespace AI.SupportTriage.Tests.Services;

public sealed class RuleBasedTriageServiceTests
{
    private readonly RuleBasedTriageService _service = new();

    [Theory]
    [InlineData("My Visa payment failed", SupportCategory.Payments)]
    [InlineData("I forgot my login password", SupportCategory.Account)]
    [InlineData("The app has a crash error", SupportCategory.Technical)]
    [InlineData("I need an invoice refund", SupportCategory.Billing)]
    [InlineData("Where is my shipping tracking?", SupportCategory.Shipping)]
    [InlineData("I would like information about your company", SupportCategory.Other)]
    public async Task AnalyzeAsync_ClassifiesCategory(string message, SupportCategory expected)
    {
        var result = await AnalyzeAsync(message);

        Assert.Equal(expected, result.Category);
    }

    [Theory]
    [InlineData("This is urgent and must be done today", SupportPriority.High)]
    [InlineData("Critical emergency in production", SupportPriority.Critical)]
    [InlineData("Just a question, this is not urgent", SupportPriority.Low)]
    [InlineData("Please review this request", SupportPriority.Medium)]
    public async Task AnalyzeAsync_ClassifiesPriority(string message, SupportPriority expected)
    {
        var result = await AnalyzeAsync(message);

        Assert.Equal(expected, result.Priority);
    }

    [Theory]
    [InlineData("This service is unacceptable and I am furious", SupportSentiment.Angry)]
    [InlineData("I am frustrated because it doesn't work", SupportSentiment.Frustrated)]
    [InlineData("Thanks, your support is excellent", SupportSentiment.Positive)]
    [InlineData("I need information about the service", SupportSentiment.Neutral)]
    public async Task AnalyzeAsync_ClassifiesSentiment(string message, SupportSentiment expected)
    {
        var result = await AnalyzeAsync(message);

        Assert.Equal(expected, result.Sentiment);
    }

    [Theory]
    [InlineData("payment", "Payments Support")]
    [InlineData("account", "Account Support")]
    [InlineData("technical", "Technical Support")]
    [InlineData("billing", "Billing Support")]
    [InlineData("shipping", "Logistics Support")]
    [InlineData("unknown topic", "General Support")]
    public async Task AnalyzeAsync_MapsSuggestedTeam(string message, string expected)
    {
        var result = await AnalyzeAsync(message);

        Assert.Equal(expected, result.SuggestedTeam);
    }

    [Fact]
    public async Task AnalyzeAsync_PreservesShortSummaryAndNormalizesWhitespace()
    {
        const string message = "A short\n\n support   message.";

        var result = await AnalyzeAsync(message);

        Assert.Equal("A short support message.", result.Summary);
    }

    [Fact]
    public async Task AnalyzeAsync_TruncatesLongSummary()
    {
        var message = new string('a', 200);

        var result = await AnalyzeAsync(message);

        Assert.Equal(180, result.Summary.Length);
        Assert.EndsWith("...", result.Summary);
    }

    [Fact]
    public async Task AnalyzeAsync_ClassifiesMainExample()
    {
        const string message = "No puedo completar el pago con Visa y necesito resolverlo hoy.";

        var result = await AnalyzeAsync(message);

        Assert.Equal(SupportCategory.Payments, result.Category);
        Assert.Equal(SupportPriority.High, result.Priority);
        Assert.Equal(SupportSentiment.Frustrated, result.Sentiment);
        Assert.Equal("Payments Support", result.SuggestedTeam);
    }

    private Task<TriageResult> AnalyzeAsync(string message)
    {
        return _service.AnalyzeAsync(new TriageRequest { Message = message });
    }
}
