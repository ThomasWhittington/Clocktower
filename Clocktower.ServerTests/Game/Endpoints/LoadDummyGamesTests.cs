using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class LoadDummyGamesTests
{
    private Mock<IGamePerspectiveService> _mockGamePerspectiveService = null!;
    private const string ResponseMessage = "Response";

    private void MockResponse(bool success)
    {
        _mockGamePerspectiveService.Setup(o =>
                o.LoadDummyData())
            .Returns((success, ResponseMessage));
    }

    [TestInitialize]
    public void Setup()
    {
        _mockGamePerspectiveService = new Mock<IGamePerspectiveService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        LoadDummyGames.Map(builder);

        builder.GetEndpoint("/load")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("loadDummyGamesApi")
            .ShouldHaveSummaryAndDescription("Loads dummy data from saved json file");
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenServiceLoadDummyGamesReturnsFalse()
    {
        MockResponse(false);

        var result = LoadDummyGames.Handle(_mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.LoadDummyData(), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(ResponseMessage);
    }

    [TestMethod]
    public void Handle_ReturnsCreated_WhenServiceLoadDummyGamesReturnsTrue()
    {
        MockResponse(true);

        var result = LoadDummyGames.Handle(_mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.LoadDummyData(), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(ResponseMessage);
    }
}