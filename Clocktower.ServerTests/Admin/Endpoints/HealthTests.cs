using Clocktower.Server.Admin.Endpoints;

namespace Clocktower.ServerTests.Admin.Endpoints;

[TestClass]
public class HealthTests
{
    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        Health.Map(builder);

        builder.GetEndpoint("/health")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("healthApi")
            .ShouldHaveSummaryAndDescription("Checks the health of the server");
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