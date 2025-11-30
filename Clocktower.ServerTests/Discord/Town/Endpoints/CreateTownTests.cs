using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class CreateTownTests
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

        CreateTown.Map(builder);

        builder.GetEndpoint("/{guildId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("createTownApi")
            .ShouldHaveSummary("Creates the town in the provided server")
            .ShouldHaveDescription("Creates the roles, categories and channels required for clocktower")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        const string responseMessage = "response message";
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());

        _mockDiscordTownService.Setup(o => o.CreateTown(ulong.Parse(request.GuildId))).ReturnsAsync((false, responseMessage));

        var result = await CreateTown.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.CreateTown(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        const string responseMessage = "response message";
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());

        _mockDiscordTownService.Setup(o => o.CreateTown(ulong.Parse(request.GuildId))).ReturnsAsync((true, responseMessage));

        var result = await CreateTown.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.CreateTown(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(responseMessage);
    }
}