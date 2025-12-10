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

        builder.GetEndpoint("/{guildId}/occupancy")
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
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());
        _mockDiscordTownService.Setup(o => o.GetDiscordTown(ulong.Parse(request.GuildId))).ReturnsAsync((false, null, responseMessage));

        var result = await GetDiscordTown.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.GetDiscordTown(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        const string responseMessage = "response message";
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());
        var discordTown = new DiscordTown([new MiniCategory(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [])]);
        _mockDiscordTownService.Setup(o => o.GetDiscordTown(ulong.Parse(request.GuildId))).ReturnsAsync((true, discordTown, responseMessage));

        var result = await GetDiscordTown.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.GetDiscordTown(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<DiscordTown>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(discordTown);
    }
}