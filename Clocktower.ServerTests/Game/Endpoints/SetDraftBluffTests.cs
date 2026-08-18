using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class SetDraftBluffTests
{
    private Mock<IGameService> _mockGameService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockGameService = new Mock<IGameService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        SetDraftBluff.Map(builder);

        builder.GetEndpoint("/{gameId}/{userId}/set-draft-bluff/{slot}/{roleId?}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("setDraftBluffApi")
            .ShouldHaveSummaryAndDescription("Sets a draft bluff for a player in a game")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsInvalidError()
    {
        var request = new SetDraftBluff.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), 1, CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");

        _mockGameService.Setup(o => o.UpdateDraftBluff(request.GameId, request.UserId, request.Slot, request.RoleId)).ReturnsAsync(error);

        var result = await SetDraftBluff.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.UpdateDraftBluff(request.GameId, request.UserId, request.Slot, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError()
    {
        var request = new SetDraftBluff.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), 2, CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");

        _mockGameService.Setup(o => o.UpdateDraftBluff(request.GameId, request.UserId, request.Slot, request.RoleId)).ReturnsAsync(error);

        var result = await SetDraftBluff.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.UpdateDraftBluff(request.GameId, request.UserId, request.Slot, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsOk()
    {
        var request = new SetDraftBluff.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), 3, CommonMethods.GetRandomString());
        var success = Result.Ok("success");

        _mockGameService.Setup(o => o.UpdateDraftBluff(request.GameId, request.UserId, request.Slot, request.RoleId)).ReturnsAsync(success);

        var result = await SetDraftBluff.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.UpdateDraftBluff(request.GameId, request.UserId, request.Slot, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenRoleIdIsNull()
    {
        var request = new SetDraftBluff.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), 1, null);
        var success = Result.Ok("success");

        _mockGameService.Setup(o => o.UpdateDraftBluff(request.GameId, request.UserId, request.Slot, null)).ReturnsAsync(success);

        var result = await SetDraftBluff.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.UpdateDraftBluff(request.GameId, request.UserId, request.Slot, null), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}