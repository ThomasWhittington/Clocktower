using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class DeleteTownTests
{
    private Mock<IDiscordTownService> _mockDiscordTownService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockDiscordTownService = new Mock<IDiscordTownService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        DeleteTown.Map(builder);

        builder.GetEndpoint("/{guildId}")
            .ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHaveOperationId("deleteTownApi")
            .ShouldHaveSummary("Deletes the town in the provided server")
            .ShouldHaveDescription("Removes all roles, channels and categories associated with clocktower")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        const string responseMessage = "response message";
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());

        _mockDiscordTownService.Setup(o => o.DeleteTown(ulong.Parse(request.GuildId))).ReturnsAsync((false, responseMessage));

        var result = await DeleteTown.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.DeleteTown(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        const string responseMessage = "response message";
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());

        _mockDiscordTownService.Setup(o => o.DeleteTown(ulong.Parse(request.GuildId))).ReturnsAsync((true, responseMessage));

        var result = await DeleteTown.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.DeleteTown(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(responseMessage);
    }
}