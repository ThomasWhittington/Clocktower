using Clocktower.Server.Data;
using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class GetTownOccupancyTests
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

        GetTownOccupancy.Map(builder);

        builder.GetEndpoint("/{guildId}/occupancy")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("getTownOccupancyApi")
            .ShouldHaveSummary("Get occupancy of town")
            .ShouldHaveDescription("Gets user presense in the town")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        const string responseMessage = "response message";
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());
        _mockDiscordTownService.Setup(o => o.GetTownOccupancy(ulong.Parse(request.GuildId))).ReturnsAsync((false, null, responseMessage));

        var result = await GetTownOccupancy.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.GetTownOccupancy(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        const string responseMessage = "response message";
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());
        var townOccupants = new TownOccupants([new MiniCategory(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [])]);
        _mockDiscordTownService.Setup(o => o.GetTownOccupancy(ulong.Parse(request.GuildId))).ReturnsAsync((true, townOccupants, responseMessage));

        var result = await GetTownOccupancy.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.GetTownOccupancy(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<TownOccupants>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(townOccupants);
    }
}