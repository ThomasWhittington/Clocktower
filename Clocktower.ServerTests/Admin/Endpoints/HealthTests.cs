using Clocktower.Server.Admin.Endpoints;

namespace Clocktower.ServerTests.Admin.Endpoints;

using Health = Health;

[TestClass]
public class HealthTests
{
    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        Health.Map(builder);

        var endpoint = builder.GetEndpoint("/health");

        endpoint.ShouldHaveMethod(HttpMethod.Get);
        endpoint.ShouldHaveOperationId("healthApi");
        endpoint.ShouldHaveSummary("Checks the health of the server");
        endpoint.ShouldHaveDescription("Checks the health of the server");
        endpoint.ShouldHaveEndpointName("Clocktower.Server.Admin.Endpoints.Health");
    }

    [TestMethod]
    public void Handle_ReturnsOk_WithHealthyStatus()
    {
        var beforeTest = DateTime.UtcNow;
        var result = Health.Handle();

        var okResult = result.Should().BeOfType<Ok<Health.Response>>().Subject.Value;
        okResult.Should().NotBeNull();
        okResult.Status.Should().Be("Healthy");
        okResult.TimeStamp.Should().BeCloseTo(beforeTest, TimeSpan.FromSeconds(1));
    }
}