using Clocktower.Server.Discord.GameAction.Endpoints;
using Clocktower.Server.Discord.GameAction.Services;
using Clocktower.Server.Discord.Town.Endpoints.Validation;

namespace Clocktower.ServerTests.Discord.GameAction.Endpoints;

[TestClass]
public class SendToTownSquareTests
{
    private Mock<IDiscordGameActionService> _mockDiscordGameActionService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockDiscordGameActionService = new Mock<IDiscordGameActionService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        SendToTownSquare.Map(builder);

        builder.GetEndpoint("/send-to-townsquare/{gameId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("sendToTownSquareApi")
            .ShouldHaveSummary("Sends all users to townsquare")
            .ShouldHaveDescription("Sends all users to townsquare")
            .ShouldHaveValidation();
    }

    [TestMethod]
         public async Task Handle_ReturnsBadRequest_WhenServiceReturnsBadRequestError()
         {
             const string gameId = "game-id";
             var request = new GameIdRequest(gameId);
             var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");
             _mockDiscordGameActionService.Setup(o => o.SendToTownSquareAsync(gameId)).ReturnsAsync(error);
     
             var result = await SendToTownSquare.Handle(request, _mockDiscordGameActionService.Object);
     
             _mockDiscordGameActionService.Verify(o => o.SendToTownSquareAsync(gameId), Times.Once);
             var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
             response.Value.ShouldBeError(error);
         }
     
     
         [TestMethod]
         public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError()
         {
             const string gameId = "game-id";
             var request = new GameIdRequest(gameId);
             var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");
             _mockDiscordGameActionService.Setup(o => o.SendToTownSquareAsync(gameId)).ReturnsAsync(error);
     
             var result = await SendToTownSquare.Handle(request, _mockDiscordGameActionService.Object);
     
             _mockDiscordGameActionService.Verify(o => o.SendToTownSquareAsync(gameId), Times.Once);
             var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
             response.Value.ShouldBeError(error);
         }
     
         [TestMethod]
         public async Task Handle_ReturnsOk_WhenServiceSendsInvite()
         {
             const string gameId = "game-id";
             var request = new GameIdRequest(gameId);
             var success = Result.Ok("response message");
             _mockDiscordGameActionService.Setup(o => o.SendToTownSquareAsync(gameId)).ReturnsAsync(success);
     
             var result = await SendToTownSquare.Handle(request, _mockDiscordGameActionService.Object);
     
             _mockDiscordGameActionService.Verify(o => o.SendToTownSquareAsync(gameId), Times.Once);
             var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
             response.Value.Should().Be(success.Value);
         }
}