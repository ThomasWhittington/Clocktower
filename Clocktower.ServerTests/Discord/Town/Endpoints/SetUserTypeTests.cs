using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class SetUserTypeTests
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

        SetUserType.Map(builder);

        builder.GetEndpoint("/{gameId}/{userId}/set-type/{userType}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("setUserTypeApi")
            .ShouldHaveSummaryAndDescription("Sets the userType for a user")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsBadRequestError()
    {
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");
        const string gameId = "game-id";
        const string userId = "user-id";
        const UserType userType = UserType.Player;
        var request = new SetUserType.Request(gameId, userId, userType);
        _mockDiscordTownService.Setup(o => o.SetUserType(request.GameId, request.UserId, request.UserType)).ReturnsAsync(error);

        var result = await SetUserType.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.SetUserType(request.GameId, request.UserId, request.UserType), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError()
    {
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");
        const string gameId = "game-id";
        const string userId = "user-id";
        const UserType userType = UserType.Player;
        var request = new SetUserType.Request(gameId, userId, userType);
        _mockDiscordTownService.Setup(o => o.SetUserType(request.GameId, request.UserId, request.UserType)).ReturnsAsync(error);

        var result = await SetUserType.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.SetUserType(request.GameId, request.UserId, request.UserType), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsOk()
    {
        var success = Result.Ok("response message");
        const string gameId = "game-id";
        const string userId = "user-id";
        const UserType userType = UserType.Player;
        var request = new SetUserType.Request(gameId, userId, userType);
        _mockDiscordTownService.Setup(o => o.SetUserType(request.GameId, request.UserId, request.UserType)).ReturnsAsync(success);

        var result = await SetUserType.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.SetUserType(request.GameId, request.UserId, request.UserType), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().Be(success.Value);
    }
}