using Clocktower.Server.Admin.Services;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Types.Enum;

namespace Clocktower.ServerTests.Admin.Services;

[TestClass]
public class AdminServiceTests
{
    private IAdminService _sut = null!;
    private Mock<IJwtWriter> _mockJwtWriter = null!;

    private void SetUpJwtWriter(string jwt)
    {
        _mockJwtWriter.Setup(o => o.GetJwtToken(It.Is<GameUser>(user =>
            user.Id == "0" &&
            user.UserType == UserType.StoryTeller
        ), isTest: true)).Returns(jwt);
    }

    [TestInitialize]
    public void SetUp()
    {
        _mockJwtWriter = new Mock<IJwtWriter>();
        _sut = new AdminService(_mockJwtWriter.Object);
    }

    [TestMethod]
    public void GenerateJwtToken_ReturnsTrue_WhenJwtReceived()
    {
        const string username = "username";
        const string jwt = "this-jwt";
        SetUpJwtWriter(jwt);

        var (success, result) = _sut.GenerateJwtToken(username);

        _mockJwtWriter.Verify(o => o.GetJwtToken(It.Is<GameUser>(user =>
            user.Id == "0" &&
            user.UserType == UserType.StoryTeller
        ), isTest: true), Times.Once);
        success.Should().BeTrue();
        result.Should().Be(jwt);
    }

    [TestMethod]
    public void GenerateJwtToken_ReturnsFalse_WhenExceptionThrow()
    {
        const string exceptionMessage = "exception-message";
        _mockJwtWriter.Setup(o => o.GetJwtToken(It.IsAny<GameUser>(), It.IsAny<bool>())).Throws(new Exception(exceptionMessage));

        var (success, result) = _sut.GenerateJwtToken(It.IsAny<string>());

        success.Should().BeFalse();
        result.Should().Be(exceptionMessage);
    }
}