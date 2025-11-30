using Clocktower.Server.Common;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Discord.Auth.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Clocktower.ServerTests.Discord.Auth.Services;

[TestClass]
public class DiscordAuthServiceTests
{
    private Mock<IOptions<Secrets>> _mockSecrets = null!;
    private Mock<IJwtWriter> _mockJwtWriter = null!;
    private Mock<IMemoryCache> _mockCache = null!;
    private Mock<IHttpClientFactory> _mockHttpClientFactory = null!;
    private Mock<IDiscordAuthApiService> _mockDiscordAuthApiService = null!;
    private Mock<IIdGenerator> _mockIdGenerator = null!;

    private HttpClient _testHttpClient = null!;

    private IDiscordAuthService Sut => new DiscordAuthService(
        _mockSecrets.Object,
        _mockJwtWriter.Object,
        _mockCache.Object,
        _mockHttpClientFactory.Object,
        _mockDiscordAuthApiService.Object,
        _mockIdGenerator.Object
    );

    [TestInitialize]
    public void Setup()
    {
        _mockSecrets = new Mock<IOptions<Secrets>>();
        _mockJwtWriter = new Mock<IJwtWriter>();
        _mockCache = new Mock<IMemoryCache>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockDiscordAuthApiService = new Mock<IDiscordAuthApiService>();
        _mockIdGenerator = new Mock<IIdGenerator>();

        _testHttpClient = new HttpClient();
    }

    #region GetAuthorizationUrl

    [TestMethod]
    public void GetAuthorizationUrl_ReturnsFalse_WhenDiscordAuthNotConfigured()
    {
        CommonMethods.SetUpMockSecrets(_mockSecrets);

        var (success, url, message) = Sut.GetAuthorizationUrl();

        success.Should().BeFalse();
        url.Should().BeEmpty();
        message.Should().Be("Discord OAuth not properly configured");
    }

    [TestMethod]
    public void GetAuthorizationUrl_ReturnsTrue_WhenDiscordAuthConfigured()
    {
        const string discordClientId = "discordClientId";
        const string serverUri = "serverUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            discordClientId: discordClientId,
            serverUri: serverUri
        );

        var expectedUrl = $"https://discord.com/api/oauth2/authorize" +
                          $"?client_id={discordClientId}" +
                          $"&redirect_uri={Uri.EscapeDataString(serverUri + "/api/discord/auth/callback")}" +
                          $"&response_type=code" +
                          $"&scope={Uri.EscapeDataString("identify guilds")}";

        var (success, url, message) = Sut.GetAuthorizationUrl();

        success.Should().BeTrue();
        url.Should().Be(expectedUrl);
        message.Should().Be("Authorization Url generated");
    }

    #endregion

    #region GetAddBotUrl

    [TestMethod]
    public void GetAddBotUrl_ReturnsFalse_WhenDiscordAuthNotConfigured()
    {
        CommonMethods.SetUpMockSecrets(_mockSecrets);

        var (success, url, message) = Sut.GetAddBotUrl();

        success.Should().BeFalse();
        url.Should().BeEmpty();
        message.Should().Be("Discord OAuth not properly configured");
    }

    [TestMethod]
    public void GetAddBotUrl_ReturnsTrue_WhenDiscordAuthConfigured()
    {
        const int permissionsInt = 8;
        const string discordClientId = "discordClientId";
        const string serverUri = "serverUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            discordClientId: discordClientId,
            serverUri: serverUri
        );

        var expectedUrl = $"https://discord.com/oauth2/authorize" +
                          $"?client_id={discordClientId}" +
                          $"&permissions={permissionsInt}" +
                          $"&integration_type=0" +
                          $"&scope=bot" +
                          $"&response_type=code" +
                          $"&redirect_uri={Uri.EscapeDataString(serverUri + "/api/discord/auth/bot-callback")}";

        var (success, url, message) = Sut.GetAddBotUrl();

        success.Should().BeTrue();
        url.Should().Be(expectedUrl);
        message.Should().Be("Bot addition Url generated");
    }

    #endregion

    #region GetAuthData

    [TestMethod]
    public void GetAuthData_CallsCacheTryGetValue_ReturnsNull_Always()
    {
        const string key = "this-key";
        object invalidValue = null!;
        _mockCache.Setup(o => o.TryGetValue($"auth_data_{key}", out invalidValue!)).Returns(false);

        var result = Sut.GetAuthData(key);

        _mockCache.Verify(o => o.TryGetValue($"auth_data_{key}", out It.Ref<object?>.IsAny), Times.Once);
        result.Should().BeNull();
    }

    [TestMethod]
    public void GetAuthData_ReturnsNull_WhenCacheValueInvalid()
    {
        const string key = "this-key";
        object invalidValue = "not-a-user-auth-data-object";
        _mockCache.Setup(o => o.TryGetValue($"auth_data_{key}", out invalidValue!)).Returns(true);

        var result = Sut.GetAuthData(key);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetAuthData_CallsCacheRemove_ReturnsData_WhenCacheValueFound()
    {
        const string key = "this-key";
        object authData = new UserAuthData(CommonMethods.GetRandomGameUser(), CommonMethods.GetRandomString());
        _mockCache.Setup(o => o.TryGetValue($"auth_data_{key}", out authData!)).Returns(true);

        var result = Sut.GetAuthData(key);

        _mockCache.Verify(o => o.Remove($"auth_data_{key}"), Times.Once);
        result.Should().Be(authData);
    }

    #endregion

    #region HandleCallback

    [TestMethod]
    public async Task HandleCallback_ReturnsErrorUrl_WhenErrorProvided()
    {
        const string feUri = "feUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            feUri: feUri
        );

        const string error = "this-error";
        const string code = "this-code";

        var result = await Sut.HandleCallback(error, code);

        result.Should().Be(feUri + "/login?error=" + Uri.EscapeDataString($"Discord OAuth error: {error}"));
    }

    [TestMethod]
    public async Task HandleCallback_ReturnsErrorUrl_WhenCodeNotProvided()
    {
        const string feUri = "feUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            feUri: feUri
        );

        const string error = null!;
        const string code = null!;

        var result = await Sut.HandleCallback(error, code);

        result.Should().Be(feUri + "/login?error=" + Uri.EscapeDataString("Authorization code not received"));
    }

    [TestMethod]
    public async Task HandleCallback_ReturnsErrorUrl_WhenFailedToGetToken()
    {
        const string feUri = "feUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            feUri: feUri
        );
        const string error = null!;
        const string code = "this-code";

        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(_testHttpClient);
        _mockDiscordAuthApiService.Setup(o => o.ExchangeCodeForToken(code, _testHttpClient)).ReturnsAsync((TokenResponse?)null);

        var result = await Sut.HandleCallback(error, code);

        _mockHttpClientFactory.Verify(x => x.CreateClient(string.Empty), Times.Once);
        _mockDiscordAuthApiService.Verify(o => o.ExchangeCodeForToken(code, _testHttpClient), Times.Once);

        result.Should().Be(feUri + "/login?error=" + Uri.EscapeDataString("Failed to exchange code for token"));
    }

    [TestMethod]
    public async Task HandleCallback_ReturnsErrorUrl_WhenFailedToGetUserInfo()
    {
        const string feUri = "feUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            feUri: feUri
        );
        const string error = null!;
        const string code = "this-code";

        var tokenResponse = new TokenResponse
        {
            AccessToken = CommonMethods.GetRandomString(),
            TokenType = CommonMethods.GetRandomString(),
            ExpiresIn = 0,
            RefreshToken = CommonMethods.GetRandomString(),
            Scope = CommonMethods.GetRandomString()
        };

        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(_testHttpClient);
        _mockDiscordAuthApiService.Setup(o => o.ExchangeCodeForToken(code, _testHttpClient)).ReturnsAsync(tokenResponse);
        _mockDiscordAuthApiService.Setup(x => x.GetDiscordUserInfo(tokenResponse.AccessToken, _testHttpClient))
            .ReturnsAsync((DiscordUser?)null);

        var result = await Sut.HandleCallback(error, code);

        _mockHttpClientFactory.Verify(x => x.CreateClient(string.Empty), Times.Once);
        _mockDiscordAuthApiService.Verify(o => o.ExchangeCodeForToken(code, _testHttpClient), Times.Once);
        _mockDiscordAuthApiService.Verify(o => o.GetDiscordUserInfo(tokenResponse.AccessToken, _testHttpClient), Times.Once);

        result.Should().Be(feUri + "/login?error=" + Uri.EscapeDataString("Failed to get user information"));
    }

    [TestMethod]
    public async Task HandleCallback_CallsGetJwtToken_WhenDataIsGood()
    {
        const string feUri = "feUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            feUri: feUri
        );
        const string error = null!;
        const string code = "this-code";

        var tokenResponse = new TokenResponse
        {
            AccessToken = CommonMethods.GetRandomString(),
            TokenType = CommonMethods.GetRandomString(),
            ExpiresIn = 0,
            RefreshToken = CommonMethods.GetRandomString(),
            Scope = CommonMethods.GetRandomString()
        };
        var userInfo = new DiscordUser(
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomString(),
            false,
            CommonMethods.GetRandomString()
        );

        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(_testHttpClient);
        _mockDiscordAuthApiService.Setup(o => o.ExchangeCodeForToken(code, _testHttpClient)).ReturnsAsync(tokenResponse);
        _mockDiscordAuthApiService.Setup(x => x.GetDiscordUserInfo(tokenResponse.AccessToken, _testHttpClient))
            .ReturnsAsync(userInfo);

        _ = await Sut.HandleCallback(error, code);

        _mockJwtWriter.Verify(o => o.GetJwtToken(It.IsAny<GameUser>()), Times.Once);
    }


    [TestMethod]
    public async Task HandleCallback_GeneratesId_WhenDataIsGood()
    {
        const string feUri = "feUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            feUri: feUri
        );
        const string error = null!;
        const string code = "this-code";
        const string key = "this-key";

        var tokenResponse = new TokenResponse
        {
            AccessToken = CommonMethods.GetRandomString(),
            TokenType = CommonMethods.GetRandomString(),
            ExpiresIn = 0,
            RefreshToken = CommonMethods.GetRandomString(),
            Scope = CommonMethods.GetRandomString()
        };
        var userInfo = new DiscordUser(
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomString(),
            false,
            CommonMethods.GetRandomString()
        );
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_testHttpClient);
        _mockDiscordAuthApiService.Setup(o => o.ExchangeCodeForToken(code, _testHttpClient)).ReturnsAsync(tokenResponse);
        _mockDiscordAuthApiService.Setup(x => x.GetDiscordUserInfo(tokenResponse.AccessToken, _testHttpClient)).ReturnsAsync(userInfo);
        _mockIdGenerator.Setup(o => o.GenerateId()).Returns(key);

        _ = await Sut.HandleCallback(error, code);

        _mockIdGenerator.Verify(o => o.GenerateId(), Times.Once);
    }

    [TestMethod]
    public async Task HandleCallback_SetsCacheAndReturnsUrl_WhenGotId()
    {
        const string feUri = "feUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            feUri: feUri
        );
        const string error = null!;
        const string code = "this-code";
        const string jwt = "this-jwt";
        const string key = "this-key";

        var tokenResponse = new TokenResponse
        {
            AccessToken = CommonMethods.GetRandomString(),
            TokenType = CommonMethods.GetRandomString(),
            ExpiresIn = 0,
            RefreshToken = CommonMethods.GetRandomString(),
            Scope = CommonMethods.GetRandomString()
        };
        var userInfo = new DiscordUser(
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomString(),
            false,
            CommonMethods.GetRandomString()
        );

        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(_testHttpClient);
        _mockDiscordAuthApiService.Setup(o => o.ExchangeCodeForToken(code, _testHttpClient)).ReturnsAsync(tokenResponse);
        _mockDiscordAuthApiService.Setup(x => x.GetDiscordUserInfo(tokenResponse.AccessToken, _testHttpClient))
            .ReturnsAsync(userInfo);
        _mockJwtWriter.Setup(o => o.GetJwtToken(It.Is<GameUser>(g => g.Id == userInfo.Id))).Returns(jwt);
        _mockIdGenerator.Setup(o => o.GenerateId()).Returns(key);
        var mockCacheEntry = new Mock<ICacheEntry>();
        _mockCache.Setup(o => o.CreateEntry(It.IsAny<object>()))
            .Returns(mockCacheEntry.Object);

        var result = await Sut.HandleCallback(error, code);

        result.Should().Contain(feUri);
        _mockCache.Verify(o => o.CreateEntry(It.Is<object>(e => e.ToString()!.Equals($"auth_data_{key}"))), Times.Once);
        mockCacheEntry.VerifySet(entry => entry.Value = It.Is<UserAuthData>(u => u.Jwt == jwt && u.GameUser.Id == userInfo.Id), Times.Once);
        mockCacheEntry.VerifySet(entry => entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), Times.Once);
    }

    #endregion

    #region HandleBotCallback

    [TestMethod]
    public void HandleBotCallback_ReturnsErrorUrl_WhenErrorProvided()
    {
        const string feUri = "feUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            feUri: feUri
        );
        const string error = "this-error";
        const string code = null!;
        const string guildId = null!;

        var result = Sut.HandleBotCallback(error, code, guildId);

        result.Should().Be(feUri + "/login?error=" + Uri.EscapeDataString($"Discord OAuth error: {error}"));
    }

    [TestMethod]
    public void HandleBotCallback_ReturnsErrorUrl_WhenCodeNotProvided()
    {
        const string feUri = "feUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            feUri: feUri
        );
        const string error = null!;
        const string code = null!;
        const string guildId = "guildId";

        var result = Sut.HandleBotCallback(error, code, guildId);

        result.Should().Be(feUri + "/login?error=" + Uri.EscapeDataString("Authorization code not received"));
    }

    [TestMethod]
    public void HandleBotCallback_ReturnsErrorUrl_WhenGuildIdNotProvided()
    {
        const string feUri = "feUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            feUri: feUri
        );
        const string error = null!;
        const string code = "this-code";
        const string guildId = null!;

        var result = Sut.HandleBotCallback(error, code, guildId);

        result.Should().Be(feUri + "/login?error=" + Uri.EscapeDataString("No guildId received"));
    }

    [TestMethod]
    public void HandleBotCallback_ReturnsUrl_WhenDataIsGood()
    {
        const string feUri = "feUri";
        CommonMethods.SetUpMockSecrets(_mockSecrets,
            feUri: feUri
        );
        const string error = null!;
        const string code = "this-code";
        const string guildId = "guildId";

        var result = Sut.HandleBotCallback(error, code, guildId);

        result.Should().Be(feUri + "/auth/bot-callback?guild_id=" + guildId);
    }

    #endregion
}