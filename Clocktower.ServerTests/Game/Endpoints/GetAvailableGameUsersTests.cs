using Clocktower.Server.Data.Dto;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class GetAvailableGameUsersTests
{
    private Mock<IGamePerspectiveService> _mockGamePerspectiveService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockGamePerspectiveService = new Mock<IGamePerspectiveService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        GetAvailableGameUsers.Map(builder);

        builder.GetEndpoint("/{gameId}/available-users")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("getAvailableGameUsersApi")
            .ShouldHaveSummary("Get available guild users for a game")
            .ShouldHaveDescription("Gets all users that are in the games guild but are not yet added to the game")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        var request = new GameIdRequest(CommonMethods.GetRandomString());

        var error = Result.Fail<IEnumerable<UserDto>>(ErrorKind.Invalid, "error code", "error message");
        _mockGamePerspectiveService.Setup(o => o.GetAvailableGameUsers(request.GameId)).Returns(error);

        var result = GetAvailableGameUsers.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetAvailableGameUsers(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public void Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        var request = new GameIdRequest(CommonMethods.GetRandomString());
        var expected = new List<UserDto>
        {
            new("id", "name", "avatar")
        };
        var success = Result.Ok(expected.AsEnumerable());

        _mockGamePerspectiveService.Setup(o => o.GetAvailableGameUsers(request.GameId)).Returns(success);

        var result = GetAvailableGameUsers.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetAvailableGameUsers(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<IEnumerable<UserDto>>>().Subject;
        response.Value.Should().BeEquivalentTo(expected);
    }
}