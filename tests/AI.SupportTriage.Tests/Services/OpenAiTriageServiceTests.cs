using AI.SupportTriage.Api.Contracts;
using AI.SupportTriage.Api.Domain;
using AI.SupportTriage.Api.Services;

namespace AI.SupportTriage.Tests.Services;

public sealed class OpenAiTriageServiceTests
{
    private const string ValidOutput = """
        {
          "category": "Payments",
          "priority": "High",
          "sentiment": "Frustrated",
          "summary": "The customer cannot complete a Visa payment.",
          "suggestedTeam": "Payments Support"
        }
        """;

    [Fact]
    public async Task AnalyzeAsync_ValidStructuredOutput_ReturnsCompleteResult()
    {
        var client = StubOpenAiResponseClient.Returning(ValidOutput);
        var service = new OpenAiTriageService(client, "configured-model");

        var result = await service.AnalyzeAsync(
            new TriageRequest { Message = "Visa payment failed" });

        Assert.Equal(SupportCategory.Payments, result.Category);
        Assert.Equal(SupportPriority.High, result.Priority);
        Assert.Equal(SupportSentiment.Frustrated, result.Sentiment);
        Assert.Equal("The customer cannot complete a Visa payment.", result.Summary);
        Assert.Equal("Payments Support", result.SuggestedTeam);
        Assert.Equal("configured-model", client.ReceivedModel);
        Assert.Equal(OpenAiTriageService.Instructions, client.ReceivedInstructions);
        Assert.Equal(OpenAiTriageService.OutputSchema, client.ReceivedSchema);
    }

    [Theory]
    [InlineData("Payments")]
    [InlineData("Account")]
    [InlineData("Technical")]
    [InlineData("Billing")]
    [InlineData("Shipping")]
    [InlineData("Other")]
    public async Task AnalyzeAsync_AcceptsValidCategory(string category)
    {
        var service = CreateService(CreateOutput(category: category));

        var result = await service.AnalyzeAsync(new TriageRequest { Message = "Ticket" });

        Assert.Equal(category, result.Category.ToString());
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("Medium")]
    [InlineData("High")]
    [InlineData("Critical")]
    public async Task AnalyzeAsync_AcceptsValidPriority(string priority)
    {
        var service = CreateService(CreateOutput(priority: priority));

        var result = await service.AnalyzeAsync(new TriageRequest { Message = "Ticket" });

        Assert.Equal(priority, result.Priority.ToString());
    }

    [Theory]
    [InlineData("Positive")]
    [InlineData("Neutral")]
    [InlineData("Frustrated")]
    [InlineData("Angry")]
    public async Task AnalyzeAsync_AcceptsValidSentiment(string sentiment)
    {
        var service = CreateService(CreateOutput(sentiment: sentiment));

        var result = await service.AnalyzeAsync(new TriageRequest { Message = "Ticket" });

        Assert.Equal(sentiment, result.Sentiment.ToString());
    }

    [Theory]
    [InlineData("Payments", "Payments Support")]
    [InlineData("Account", "Account Support")]
    [InlineData("Technical", "Technical Support")]
    [InlineData("Billing", "Billing Support")]
    [InlineData("Shipping", "Logistics Support")]
    [InlineData("Other", "General Support")]
    public async Task AnalyzeAsync_AcceptsRequiredSuggestedTeam(
        string category,
        string suggestedTeam)
    {
        var service = CreateService(CreateOutput(category: category, suggestedTeam: suggestedTeam));

        var result = await service.AnalyzeAsync(new TriageRequest { Message = "Ticket" });

        Assert.Equal(suggestedTeam, result.SuggestedTeam);
    }

    [Fact]
    public async Task AnalyzeAsync_EmptyResponse_ThrowsProviderException()
    {
        var service = CreateService(null);

        await Assert.ThrowsAsync<TriageProviderException>(() =>
            service.AnalyzeAsync(new TriageRequest { Message = "Ticket" }));
    }

    [Fact]
    public async Task AnalyzeAsync_MissingRequiredValue_ThrowsProviderException()
    {
        var service = CreateService(CreateOutput(summary: string.Empty));

        await Assert.ThrowsAsync<TriageProviderException>(() =>
            service.AnalyzeAsync(new TriageRequest { Message = "Ticket" }));
    }

    [Fact]
    public async Task AnalyzeAsync_InvalidEnum_ThrowsProviderException()
    {
        var service = CreateService(CreateOutput(category: "Unknown"));

        await Assert.ThrowsAsync<TriageProviderException>(() =>
            service.AnalyzeAsync(new TriageRequest { Message = "Ticket" }));
    }

    [Fact]
    public async Task AnalyzeAsync_InvalidJson_ThrowsProviderException()
    {
        var service = CreateService("not-json");

        await Assert.ThrowsAsync<TriageProviderException>(() =>
            service.AnalyzeAsync(new TriageRequest { Message = "Ticket" }));
    }

    [Fact]
    public async Task AnalyzeAsync_ProviderError_WrapsProviderException()
    {
        var client = new StubOpenAiResponseClient(_ =>
            Task.FromException<string?>(new InvalidOperationException("Provider failure")));
        var service = new OpenAiTriageService(client, "configured-model");

        var exception = await Assert.ThrowsAsync<TriageProviderException>(() =>
            service.AnalyzeAsync(new TriageRequest { Message = "Ticket" }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public async Task AnalyzeAsync_Cancellation_PropagatesOperationCanceledException()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var client = new StubOpenAiResponseClient(token => Task.FromCanceled<string?>(token));
        var service = new OpenAiTriageService(client, "configured-model");

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.AnalyzeAsync(
                new TriageRequest { Message = "Ticket" },
                source.Token));
    }

    private static OpenAiTriageService CreateService(string? output)
    {
        return new OpenAiTriageService(
            StubOpenAiResponseClient.Returning(output),
            "configured-model");
    }

    private static string CreateOutput(
        string category = "Other",
        string priority = "Medium",
        string sentiment = "Neutral",
        string summary = "A support request.",
        string? suggestedTeam = null)
    {
        suggestedTeam ??= category switch
        {
            "Payments" => "Payments Support",
            "Account" => "Account Support",
            "Technical" => "Technical Support",
            "Billing" => "Billing Support",
            "Shipping" => "Logistics Support",
            _ => "General Support"
        };

        return $$"""
            {
              "category": "{{category}}",
              "priority": "{{priority}}",
              "sentiment": "{{sentiment}}",
              "summary": "{{summary}}",
              "suggestedTeam": "{{suggestedTeam}}"
            }
            """;
    }

    private sealed class StubOpenAiResponseClient(
        Func<CancellationToken, Task<string?>> responseFactory) : IOpenAiResponseClient
    {
        public string? ReceivedModel { get; private set; }

        public string? ReceivedInstructions { get; private set; }

        public string? ReceivedSchema { get; private set; }

        public static StubOpenAiResponseClient Returning(string? output)
        {
            return new StubOpenAiResponseClient(_ => Task.FromResult(output));
        }

        public Task<string?> CreateResponseAsync(
            string model,
            string instructions,
            string input,
            string jsonSchema,
            CancellationToken cancellationToken)
        {
            ReceivedModel = model;
            ReceivedInstructions = instructions;
            ReceivedSchema = jsonSchema;
            return responseFactory(cancellationToken);
        }
    }
}
