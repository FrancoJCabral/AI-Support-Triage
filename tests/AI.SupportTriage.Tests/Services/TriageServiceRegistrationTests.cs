using AI.SupportTriage.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.SupportTriage.Tests.Services;

public sealed class TriageServiceRegistrationTests
{
    [Fact]
    public void AddTriageServices_RuleBasedProvider_RegistersRuleBasedService()
    {
        var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["Triage:Provider"] = "RuleBased"
        });

        using var scope = provider.CreateScope();

        Assert.IsType<RuleBasedTriageService>(scope.ServiceProvider.GetRequiredService<ITriageService>());
    }

    [Fact]
    public void AddTriageServices_OpenAiProvider_RegistersOpenAiService()
    {
        var provider = BuildProvider(new Dictionary<string, string?>
        {
            ["Triage:Provider"] = "OpenAI",
            ["OpenAI:ApiKey"] = "test-key-not-a-secret",
            ["OpenAI:Model"] = "configured-model"
        });

        using var scope = provider.CreateScope();

        Assert.IsType<OpenAiTriageService>(scope.ServiceProvider.GetRequiredService<ITriageService>());
    }

    [Fact]
    public void AddTriageServices_OpenAiWithoutApiKey_ThrowsConfigurationError()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Triage:Provider"] = "OpenAI",
            ["OpenAI:Model"] = "configured-model"
        });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddTriageServices(configuration));

        Assert.Contains("OpenAI:ApiKey", exception.Message);
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> values)
    {
        var services = new ServiceCollection();
        services.AddTriageServices(BuildConfiguration(values));
        return services.BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
