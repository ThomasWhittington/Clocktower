using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class ToggleStoryTellerTests
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

        ToggleStoryTeller.Map(builder);

        builder.GetEndpoint("/{gameId}/{userId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("toggleStoryTellerApi")
            .ShouldHaveSummary("Toggles the storyteller role for a user")
            .ShouldHaveDescription("Adds or removes the storyteller role for the specified user")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceToggleStoryTellerReturnsFalse()
    {
        const string responseMessage = "response message";
        var request = new GameAndUserRequest(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId());

        _mockDiscordTownService.Setup(o => o.ToggleStoryTeller(request.GameId.Trim(), request.UserId)).ReturnsAsync((false, responseMessage));

        var result = await ToggleStoryTeller.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.ToggleStoryTeller(request.GameId.Trim(), request.UserId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceToggleStoryTellerReturnsTrue()
    {
        const string responseMessage = "response message";
        var request = new GameAndUserRequest(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId());

        _mockDiscordTownService.Setup(o => o.ToggleStoryTeller(request.GameId.Trim(), request.UserId)).ReturnsAsync((true, responseMessage));

        var result = await ToggleStoryTeller.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.ToggleStoryTeller(request.GameId.Trim(), request.UserId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(responseMessage);
    }
}