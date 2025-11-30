using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Clocktower.Server.Common;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Types.Enum;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clocktower.ServerTests.Common.Services;

[TestClass]
public class JwtWriterTests
{
    private Mock<IOptions<Secrets>> _mockSecrets = null!;
    private JwtWriter Sut => new JwtWriter(_mockSecrets.Object);
    private Secrets _secrets = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSecrets = new Mock<IOptions<Secrets>>();
        _secrets = CommonMethods.SetUpMockSecrets(_mockSecrets,
            jwtSigningKey: "this-is-a-very-long-signing-key-for-testing-purposes-256-bits",
            jwtAudience: "test-audience",
            serverUri: "https://test-server.com"
        );
    }

    [TestMethod]
    public void GetJwtToken_ReturnsValidJwtToken_WhenCalled()
    {
        var gameUser = new GameUser("123456789", "TestUser", string.Empty)
        {
            UserType = UserType.Player
        };

        var result = Sut.GetJwtToken(gameUser);

        result.Should().NotBeNullOrEmpty();
        result.Should().MatchRegex(@"^[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+\.[A-Za-z0-9\-_]+$");
    }

    [TestMethod]
    public void GetJwtToken_ContainsCorrectClaims_WhenUserIsPlayer()
    {
        var gameUser = new GameUser("player-123", "PlayerName", string.Empty)
        {
            UserType = UserType.Player
        };

        var token = Sut.GetJwtToken(gameUser);
        var decodedToken = DecodeJwtToken(token);

        decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "player-123");
        decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "PlayerName");
        decodedToken.Claims.Should().Contain(c => c.Type == "is_storyteller" && c.Value == "false");
    }

    [TestMethod]
    public void GetJwtToken_ContainsCorrectClaims_WhenUserIsStoryTeller()
    {
        var gameUser = new GameUser("storyteller-456", "StoryTellerName", string.Empty)
        {
            UserType = UserType.StoryTeller
        };

        var token = Sut.GetJwtToken(gameUser);
        var decodedToken = DecodeJwtToken(token);

        decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "storyteller-456");
        decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "StoryTellerName");
        decodedToken.Claims.Should().Contain(c => c.Type == "is_storyteller" && c.Value == "true");
    }

    [TestMethod]
    [DataRow(UserType.Unknown, "false")]
    [DataRow(UserType.Player, "false")]
    [DataRow(UserType.StoryTeller, "true")]
    public void GetJwtToken_SetsIsStorytellerClaim_BasedOnUserType(UserType userType, string expectedClaimValue)
    {
        var gameUser = new GameUser("test-user", "TestUser", string.Empty)
        {
            UserType = userType
        };

        var token = Sut.GetJwtToken(gameUser);
        var decodedToken = DecodeJwtToken(token);

        decodedToken.Claims.Should().Contain(c => c.Type == "is_storyteller" && c.Value == expectedClaimValue);
    }

    [TestMethod]
    public void GetJwtToken_SetsCorrectIssuerAndAudience_FromSecrets()
    {
        var gameUser = new GameUser("test", "test", string.Empty)
        {
            UserType = UserType.Player
        };

        var token = Sut.GetJwtToken(gameUser);
        var decodedToken = DecodeJwtToken(token);

        decodedToken.Issuer.Should().Be(_secrets.ServerUri);
        decodedToken.Audiences.Should().Contain(_secrets.Jwt.Audience);
    }

    [TestMethod]
    public void GetJwtToken_SetsExpirationTime_To12HoursFromNow()
    {
        var gameUser = new GameUser("test", "test", string.Empty)
        {
            UserType = UserType.Player
        };
        var beforeCall = DateTime.UtcNow.TruncateToSeconds();

        var token = Sut.GetJwtToken(gameUser);
        var afterCall = DateTime.UtcNow;
        var decodedToken = DecodeJwtToken(token);

        var expectedMinExpiration = beforeCall.AddHours(12);
        var expectedMaxExpiration = afterCall.AddHours(12);

        decodedToken.ValidTo.Should().BeOnOrAfter(expectedMinExpiration);
        decodedToken.ValidTo.Should().BeOnOrBefore(expectedMaxExpiration);
    }

    [TestMethod]
    public void GetJwtToken_UsesHmacSha256Algorithm_ForSigning()
    {
        var gameUser = new GameUser("test", "test", string.Empty)
        {
            UserType = UserType.Player
        };

        var token = Sut.GetJwtToken(gameUser);
        var decodedToken = DecodeJwtToken(token);

        decodedToken.Header.Alg.Should().Be(SecurityAlgorithms.HmacSha256);
    }

    [TestMethod]
    public void GetJwtToken_ThrowsException_WhenGameUserIsNull()
    {
        Sut.Invoking(x => x.GetJwtToken(null!))
            .Should().Throw<Exception>();
    }

    [TestMethod]
    public void GetJwtToken_GeneratesDifferentTokens_ForDifferentUsers()
    {
        var user1 = new GameUser("1", "User1", string.Empty)
        {
            UserType = UserType.Player
        };
        var user2 = new GameUser("2", "User2", string.Empty)
        {
            UserType = UserType.StoryTeller
        };

        var token1 = Sut.GetJwtToken(user1);
        var token2 = Sut.GetJwtToken(user2);

        token1.Should().NotBe(token2);
    }

    [TestMethod]
    public void GetJwtToken_HandlesSpecialCharacters_InUserName()
    {
        var gameUser = new GameUser("special-123", "User@#$%^&*()_+{}|:<>?[]\\;'\",./<>?", string.Empty)
        {
            UserType = UserType.Player
        };

        var token = Sut.GetJwtToken(gameUser);
        var decodedToken = DecodeJwtToken(token);

        decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == gameUser.Name);
    }

    [TestMethod]
    public void GetJwtToken_HandlesEmptyStrings_InUserProperties()
    {
        var gameUser = new GameUser(string.Empty, string.Empty, string.Empty)
        {
            UserType = UserType.Player
        };

        var token = Sut.GetJwtToken(gameUser);
        var decodedToken = DecodeJwtToken(token);

        decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "");
        decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "");
    }

    private static JwtSecurityToken DecodeJwtToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ReadJwtToken(token);
    }
}