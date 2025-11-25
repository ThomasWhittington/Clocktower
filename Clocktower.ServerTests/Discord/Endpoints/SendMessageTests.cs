using Clocktower.Server.Discord.Endpoints;
using Clocktower.Server.Discord.Services;

namespace Clocktower.ServerTests.Discord.Endpoints;

[TestClass]
public class SendMessageTests
{
    private Mock<IDiscordService> _mockDiscordService = null!;
    private const string ResponseMessage = "Response";

    [TestInitialize]
    public void Setup()
    {
        _mockDiscordService = new Mock<IDiscordService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        SendMessage.Map(builder);

        builder.GetEndpoint("/message")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("sendMessageApi")
            .ShouldHaveSummaryAndDescription("Sends message to the user")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceSendMessageReturnsFalse()
    {
        var userId = CommonMethods.GetRandomSnowflakeNumberId();
        const string message = "send this";
        var request = new SendMessage.Request(userId.ToString(), message);
        _mockDiscordService.Setup(o => o.SendMessage(userId, message)).ReturnsAsync((false, ResponseMessage));

        var result = await SendMessage.Handle(request, _mockDiscordService.Object);

        _mockDiscordService.Verify(o => o.SendMessage(userId, message), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(ResponseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceSendMessageReturnsTrue()
    {
        var userId = CommonMethods.GetRandomSnowflakeNumberId();
        const string message = "send this";
        var request = new SendMessage.Request(userId.ToString(), message);
        _mockDiscordService.Setup(o => o.SendMessage(userId, message)).ReturnsAsync((true, ResponseMessage));

        var result = await SendMessage.Handle(request, _mockDiscordService.Object);

        _mockDiscordService.Verify(o => o.SendMessage(userId, message), Times.Once);

        var response = result.Result.Should().BeOfType<Ok>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }
}