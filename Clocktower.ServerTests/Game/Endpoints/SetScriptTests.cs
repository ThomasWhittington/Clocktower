using Clocktower.Server.Data;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class SetScriptTests
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

        SetScript.Map(builder);

        builder.GetEndpoint("/{gameId}/script")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("setScriptApi")
            .ShouldHaveSummary("Sets the script of the game")
            .ShouldHaveDescription("Sets the script of the game. Setting to custom requires a custom script file to be uploaded")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        const string json = "{}";
        var request = new SetScript.Request(CommonMethods.GetRandomString(), ScriptSelect.SectsAndViolets, json);
        var error = Result.Fail<Script>(ErrorKind.Invalid, "error code", "error message");

        _mockGameService.Setup(o => o.SetScript(request.GameId, request.ScriptSelect, request.Json)).ReturnsAsync(error);

        var result = await SetScript.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetScript(request.GameId, request.ScriptSelect, request.Json), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        const string json = "{}";
        var request = new SetScript.Request(CommonMethods.GetRandomString(), ScriptSelect.SectsAndViolets, json);
        var success = Result.Ok(new Script("Name", "Author", []));

        _mockGameService.Setup(o => o.SetScript(request.GameId, request.ScriptSelect, request.Json)).ReturnsAsync(success);

        var result = await SetScript.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetScript(request.GameId, request.ScriptSelect, request.Json), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<Script>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}