using AI.SupportTriage.Api.Domain;

namespace AI.SupportTriage.Tests.Domain;

public sealed class SupportEnumsTests
{
    [Fact]
    public void SupportCategory_ContainsExpectedValues()
    {
        Assert.Equal(
            ["Payments", "Account", "Technical", "Billing", "Shipping", "Other"],
            Enum.GetNames<SupportCategory>());
    }

    [Fact]
    public void SupportPriority_ContainsExpectedValues()
    {
        Assert.Equal(
            ["Low", "Medium", "High", "Critical"],
            Enum.GetNames<SupportPriority>());
    }

    [Fact]
    public void SupportSentiment_ContainsExpectedValues()
    {
        Assert.Equal(
            ["Positive", "Neutral", "Frustrated", "Angry"],
            Enum.GetNames<SupportSentiment>());
    }
}
