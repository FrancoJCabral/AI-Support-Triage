using System.Text.Json;
using System.Text.Json.Serialization;
using AI.SupportTriage.Api.Contracts;
using AI.SupportTriage.Api.Domain;

namespace AI.SupportTriage.Api.Services;

internal sealed class OpenAiTriageService(
    IOpenAiResponseClient responseClient,
    string model) : ITriageService
{
    internal const string Instructions =
        "Analyze the support ticket. Classify category, priority, and sentiment; write a brief objective summary; " +
        "and select the team using exactly: Payments=Payments Support, Account=Account Support, " +
        "Technical=Technical Support, Billing=Billing Support, Shipping=Logistics Support, Other=General Support. " +
        "Return only the structured result without explanations or reasoning.";

    internal const string OutputSchema = """
        {
          "type": "object",
          "properties": {
            "category": {
              "type": "string",
              "enum": ["Payments", "Account", "Technical", "Billing", "Shipping", "Other"]
            },
            "priority": {
              "type": "string",
              "enum": ["Low", "Medium", "High", "Critical"]
            },
            "sentiment": {
              "type": "string",
              "enum": ["Positive", "Neutral", "Frustrated", "Angry"]
            },
            "summary": { "type": "string" },
            "suggestedTeam": { "type": "string" }
          },
          "required": ["category", "priority", "sentiment", "summary", "suggestedTeam"],
          "additionalProperties": false
        }
        """;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public async Task<TriageResult> AnalyzeAsync(
        TriageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string? output;

        try
        {
            output = await responseClient.CreateResponseAsync(
                model,
                Instructions,
                request.Message,
                OutputSchema,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException exception)
        {
            throw new TriageProviderException("The OpenAI triage request timed out.", exception);
        }
        catch (Exception exception) when (exception is not TriageProviderException)
        {
            throw new TriageProviderException("The OpenAI triage provider request failed.", exception);
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            throw new TriageProviderException("The OpenAI triage provider returned an empty response.");
        }

        OpenAiTriageOutput parsed;

        try
        {
            parsed = JsonSerializer.Deserialize<OpenAiTriageOutput>(output, SerializerOptions)
                ?? throw new JsonException("The structured response was null.");
        }
        catch (JsonException exception)
        {
            throw new TriageProviderException("The OpenAI triage provider returned invalid JSON.", exception);
        }

        var category = ParseEnum<SupportCategory>(parsed.Category, "category");
        var priority = ParseEnum<SupportPriority>(parsed.Priority, "priority");
        var sentiment = ParseEnum<SupportSentiment>(parsed.Sentiment, "sentiment");
        var expectedTeam = GetSuggestedTeam(category);

        if (string.IsNullOrWhiteSpace(parsed.Summary))
        {
            throw new TriageProviderException("The structured triage response contains an invalid summary.");
        }

        if (!string.Equals(parsed.SuggestedTeam, expectedTeam, StringComparison.Ordinal))
        {
            throw new TriageProviderException("The structured triage response contains an invalid suggested team.");
        }

        return new TriageResult
        {
            Category = category,
            Priority = priority,
            Sentiment = sentiment,
            Summary = parsed.Summary.Trim(),
            SuggestedTeam = expectedTeam
        };
    }

    private static TEnum ParseEnum<TEnum>(string? value, string fieldName)
        where TEnum : struct, Enum
    {
        if (value is null ||
            !Enum.TryParse<TEnum>(value, ignoreCase: false, out var parsed) ||
            !Enum.IsDefined(parsed))
        {
            throw new TriageProviderException(
                $"The structured triage response contains an invalid {fieldName}.");
        }

        return parsed;
    }

    private static string GetSuggestedTeam(SupportCategory category)
    {
        return category switch
        {
            SupportCategory.Payments => "Payments Support",
            SupportCategory.Account => "Account Support",
            SupportCategory.Technical => "Technical Support",
            SupportCategory.Billing => "Billing Support",
            SupportCategory.Shipping => "Logistics Support",
            _ => "General Support"
        };
    }

    private sealed class OpenAiTriageOutput
    {
        [JsonPropertyName("category")]
        public string? Category { get; init; }

        [JsonPropertyName("priority")]
        public string? Priority { get; init; }

        [JsonPropertyName("sentiment")]
        public string? Sentiment { get; init; }

        [JsonPropertyName("summary")]
        public string? Summary { get; init; }

        [JsonPropertyName("suggestedTeam")]
        public string? SuggestedTeam { get; init; }
    }
}
