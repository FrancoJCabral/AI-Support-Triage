using System.ComponentModel.DataAnnotations;
using AI.SupportTriage.Api.Contracts;

namespace AI.SupportTriage.Tests.Contracts;

public sealed class TriageRequestTests
{
    [Fact]
    public void ValidMessage_CanBeRepresented()
    {
        var request = new TriageRequest { Message = "My payment was declined." };

        Assert.Equal("My payment was declined.", request.Message);
        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyOrWhitespaceMessage_IsInvalid(string message)
    {
        var request = new TriageRequest { Message = message };

        Assert.Contains(Validate(request), result =>
            result.MemberNames.Contains(nameof(TriageRequest.Message)));
    }

    [Fact]
    public void MessageLongerThanMaximum_IsInvalid()
    {
        var request = new TriageRequest { Message = new string('a', 4001) };

        Assert.Contains(Validate(request), result =>
            result.MemberNames.Contains(nameof(TriageRequest.Message)));
    }

    private static List<ValidationResult> Validate(TriageRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, true);
        return results;
    }
}
