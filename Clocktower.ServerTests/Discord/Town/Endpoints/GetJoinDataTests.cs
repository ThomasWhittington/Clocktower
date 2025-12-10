using Clocktower.Server.Data.Types;
using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class GetJoinDataTests
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

        GetJoinData.Map(builder);

        builder.GetEndpoint("/join/{key}")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("getJoinDataApi")
            .ShouldHaveSummary("Get temporary join data")
            .ShouldHaveDescription("Retrieves temporary join data by key");
    }

    [TestMethod]
    public void Handle_ReturnsOk_WhenJoinDataIsFound()
    {
        var key = CommonMethods.GetRandomString();
        var user =CommonMethods.GetRandomGameUser();
        var joinData = new JoinData(CommonMethods.GetRandomSnowflakeStringId(), user, CommonMethods.GetRandomString(), CommonMethods.GetRandomString());

        _mockDiscordTownService.Setup(o => o.GetJoinData(key)).Returns(joinData);

        var result = GetJoinData.Handle(key, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.GetJoinData(key), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<JoinData>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(joinData);
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenJoinDataIsNotFound()
    {
        var key = CommonMethods.GetRandomString();

        _mockDiscordTownService.Setup(o => o.GetJoinData(key)).Returns((JoinData)null!);

        var result = GetJoinData.Handle(key, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.GetJoinData(key), Times.Once);
        var response = result.Result.Should().BeOfType<NotFound>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
}