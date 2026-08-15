using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class CommitDraftTests
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

        CommitDraft.Map(builder);

        builder.GetEndpoint("/{gameId}/commit-draft")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("commitDraftApi")
            .ShouldHaveSummary("Commit draft")
            .ShouldHaveDescription("Commits the draft for all users in a game, moving them to the live fields.")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        var request = new GameIdRequest(CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");

        _mockGameService.Setup(o => o.CommitDraft(request.GameId)).ReturnsAsync(error);

        var result = await CommitDraft.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.CommitDraft(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsFalse()
    {
        var request = new GameIdRequest(CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");

        _mockGameService.Setup(o => o.CommitDraft(request.GameId)).ReturnsAsync(error);

        var result = await CommitDraft.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.CommitDraft(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        var request = new GameIdRequest(CommonMethods.GetRandomString());
        var success = Result.Ok("success");

        _mockGameService.Setup(o => o.CommitDraft(request.GameId)).ReturnsAsync(success);

        var result = await CommitDraft.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.CommitDraft(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}