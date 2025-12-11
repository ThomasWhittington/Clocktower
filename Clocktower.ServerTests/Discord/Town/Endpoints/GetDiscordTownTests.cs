using Clocktower.Server.Data;
using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class GetDiscordTownTests
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

        GetDiscordTown.Map(builder);

        builder.GetEndpoint("/{gameId}/occupancy")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("getDiscordTownApi")
            .ShouldHaveSummary("Get occupancy of town")
            .ShouldHaveDescription("Gets user presense in the town")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        const string responseMessage = "response message";
        var request = new GameIdRequest(CommonMethods.GetRandomSnowflakeStringId());
        _mockDiscordTownService.Setup(o => o.GetDiscordTownDto(request.GameId)).ReturnsAsync((false, null, responseMessage));

        var result = await GetDiscordTown.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.GetDiscordTownDto(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        const string responseMessage = "response message";
        var request = new GameIdRequest(CommonMethods.GetRandomSnowflakeStringId());
        var discordTownDto = new DiscordTownDto("game-id", [new MiniCategoryDto(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [])]);
        _mockDiscordTownService.Setup(o => o.GetDiscordTownDto(request.GameId)).ReturnsAsync((true, discordTown: discordTownDto, responseMessage));

        var result = await GetDiscordTown.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.GetDiscordTownDto(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<DiscordTownDto>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(discordTownDto);
    }
}