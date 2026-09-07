using AI.SupportTriage.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace AI.SupportTriage.Tests.Controllers;

public sealed class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsOkWithExpectedStatus()
    {
        var controller = new HealthController();

        var response = Assert.IsType<OkObjectResult>(controller.Get().Result);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("ok", response.Value?.GetType().GetProperty("status")?.GetValue(response.Value));
    }
}
