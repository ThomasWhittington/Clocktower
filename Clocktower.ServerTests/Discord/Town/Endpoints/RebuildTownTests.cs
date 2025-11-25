using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class RebuildTownTests
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

        RebuildTown.Map(builder);

        builder.GetEndpoint("/{guildId}/rebuild")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("rebuildTownApi")
            .ShouldHaveSummary("Rebuild town")
            .ShouldHaveDescription("Rebuilds the town including roles, categories and channels")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceToggleStoryTellerReturnsFalse()
    {
        const string responseMessage = "response message";
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());

        _mockDiscordTownService.Setup(o => o.RebuildTown(ulong.Parse(request.GuildId))).ReturnsAsync((false, responseMessage));

        var result = await RebuildTown.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.RebuildTown(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceToggleStoryTellerReturnsTrue()
    {
        const string responseMessage = "response message";
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());

        _mockDiscordTownService.Setup(o => o.RebuildTown(ulong.Parse(request.GuildId))).ReturnsAsync((true, responseMessage));

        var result = await RebuildTown.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.RebuildTown(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(responseMessage);
    }
}