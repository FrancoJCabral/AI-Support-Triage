using OpenAI.Responses;

namespace AI.SupportTriage.Api.Services;

public static class TriageServiceRegistration
{
    public const string RuleBasedProvider = "RuleBased";
    public const string OpenAiProvider = "OpenAI";

    public static IServiceCollection AddTriageServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["Triage:Provider"];

        if (string.Equals(provider, RuleBasedProvider, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ITriageService, RuleBasedTriageService>();
            return services;
        }

        if (!string.Equals(provider, OpenAiProvider, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Triage:Provider must be '{RuleBasedProvider}' or '{OpenAiProvider}'.");
        }

        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI:ApiKey is required when Triage:Provider is OpenAI.");
        }

        var model = configuration["OpenAI:Model"];
        if (string.IsNullOrWhiteSpace(model))
        {
            throw new InvalidOperationException(
                "OpenAI:Model is required when Triage:Provider is OpenAI.");
        }

        services.AddSingleton(new ResponsesClient(apiKey));
        services.AddSingleton<IOpenAiResponseClient, OpenAiResponsesClient>();
        services.AddScoped<ITriageService>(serviceProvider =>
            new OpenAiTriageService(
                serviceProvider.GetRequiredService<IOpenAiResponseClient>(),
                model));

        return services;
    }
}
