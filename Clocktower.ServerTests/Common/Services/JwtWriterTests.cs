using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Clocktower.Server.Common;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Data.Extensions;
using Clocktower.Server.Data.Types.Enum;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clocktower.ServerTests.Common.Services;

[TestClass]
public class JwtWriterTests
{
    private const string Id = "123456789";
    private const string UserName = "userName";
    private Mock<IOptions<Secrets>> _mockSecrets = null!;
    private Mock<IUserService> _mockUserService = null!;
    private Secrets _secrets = null!;
    private IJwtWriter _sut = null!;
    private Mock<JwtWriter> _mockHandler = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSecrets = StrictMockFactory.Create<IOptions<Secrets>>();
        _mockUserService = StrictMockFactory.Create<IUserService>();
        _secrets = CommonMethods.SetUpMockSecrets(_mockSecrets,
            jwtSigningKey: "this-is-a-very-long-signing-key-for-testing-purposes-256-bits",
            jwtAudience: "test-audience",
            serverUri: "https://test-server.com"
        );

        _mockUserService.Setup(o => o.GetUserName(It.IsAny<string>())).Returns(UserName);
        _mockHandler = new Mock<JwtWriter>(_mockSecrets.Object, _mockUserService.Object)
        {
            CallBase = true
        };

        _sut = _mockHandler.Object;
    }


    [TestMethod]
    [DynamicData(nameof(Bool2))]
    public void GetJwtToken_GameUserOverload_Calls_GetJwtTokenCorrectly(bool isStoryTeller, bool testBypass)
    {
        const string id = "id";
        const string name = "name";
        var user = CommonMethods.GetRandomGameUser(id);
        user.UserType = isStoryTeller ? UserType.StoryTeller : UserType.Unknown;
        _mockUserService.Setup(o => o.GetUserName(id)).Returns(name);

        _ = _sut.GetJwtToken(user, testBypass);

        _mockHandler.Verify(x => x.GetJwtToken(id, name, isStoryTeller, testBypass), Times.Once);
    }

    [TestMethod]
    [DynamicData(nameof(Bool1))]
    public void GetJwtToken_TownUserOverload_Calls_GetJwtTokenCorrectly(bool testBypass)
    {
        const string id = "id";
        const string name = "name";
        var user = CommonMethods.GetRandomTownUser(id, name);

        _ = _sut.GetJwtToken(user, testBypass);

        _mockHandler.Verify(x => x.GetJwtToken(id, name, false, testBypass), Times.Once);
    }

    [TestMethod]
    public void GetJwtToken_ReturnsValidJwtToken_WhenCalled()
    {
        var result = _sut.GetJwtToken(Id, UserName, true, testBypass: false);

        result.Should().NotBeNullOrEmpty();
        result.Should().MatchRegex(@"^[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+$");
    }

    [TestMethod]
    [DynamicData(nameof(Bool2))]
    public void GetJwtToken_ContainsCorrectClaims(bool isStoryTeller, bool testBypass)
    {
        string id = CommonMethods.GetRandomSnowflakeStringId();
        string name = CommonMethods.GetRandomSnowflakeStringId();

        var beforeCall = DateTime.UtcNow.TruncateToSeconds();
        var token = _sut.GetJwtToken(id, name, isStoryTeller, testBypass: testBypass);
        var afterCall = DateTime.UtcNow;

        var decodedToken = DecodeJwtToken(token);
        var expectedMinExpiration = beforeCall.AddHours(12);
        var expectedMaxExpiration = afterCall.AddHours(12);
        decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == id);
        decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == name);
        decodedToken.Claims.Should().Contain(c => c.Type == "is_storyteller" && c.Value == isStoryTeller.ToLowerString());
        decodedToken.Claims.Should().Contain(c => c.Type == "test_bypass" && c.Value == testBypass.ToLowerString());
        decodedToken.Issuer.Should().Be(_secrets.ServerUri);
        decodedToken.Audiences.Should().Contain(_secrets.Jwt.Audience);
        decodedToken.ValidTo.Should().BeOnOrAfter(expectedMinExpiration);
        decodedToken.ValidTo.Should().BeOnOrBefore(expectedMaxExpiration);
        decodedToken.Header.Alg.Should().Be(SecurityAlgorithms.HmacSha256);
    }

    private static JwtSecurityToken DecodeJwtToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ReadJwtToken(token);
    }

    public static IEnumerable<object[]> Bool1() => TestDataProvider.GenerateBooleanCombinations(1);
    public static IEnumerable<object[]> Bool2() => TestDataProvider.GenerateBooleanCombinations(2);
}