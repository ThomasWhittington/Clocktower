using System.Text.Json;
using Clocktower.Server.Common;
using Clocktower.Server.Data;
using Clocktower.Server.Discord.Auth.Services;
using Microsoft.Extensions.Options;
using Moq.Protected;

namespace Clocktower.ServerTests.Discord.Auth.Services;

[TestClass]
public class DiscordAuthApiServiceTests
{
    private Mock<IOptions<Secrets>> _mockSecrets = null!;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;
    private HttpClient _httpClient = null!;

    private IDiscordAuthApiService Sut => new DiscordAuthApiService(
        _mockSecrets.Object
    );

    [TestInitialize]
    public void Setup()
    {
        _mockSecrets = new Mock<IOptions<Secrets>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
    }

    private void SetUpSecrets(
        string? discordBotToken = null,
        string? discordClientId = null,
        string? discordClientSecret = null,
        string? serverUri = null,
        string? feUri = null,
        string? jwtSigningKey = null,
        string? jwtAudience = null
    )
    {
        var secrets = new Secrets
        {
            DiscordBotToken = discordBotToken!,
            DiscordClientId = discordClientId!,
            DiscordClientSecret = discordClientSecret!,
            ServerUri = serverUri!,
            FeUri = feUri!,
            Jwt = new JwtSecrets
            {
                SigningKey = jwtSigningKey!,
                Audience = jwtAudience!
            }
        };
        _mockSecrets.Setup(o => o.Value).Returns(secrets);
    }

    #region GetDiscordUserInfo

    [TestMethod]
    public async Task GetDiscordUserInfo_CallsDiscordApi_Always()
    {
        const string accessToken = "this-access-token";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri("https://discord.com/api/users/@me") &&
                    req.Headers.Authorization != null &&
                    req.Headers.Authorization.Scheme == "Bearer" &&
                    req.Headers.Authorization.Parameter == accessToken
                ),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"123\",\"username\":\"name\",\"avatar\":\"456\",\"discriminator\":\"0\",\"public_flags\":0,\"flags\":0,\"banner\":null,\"accent_color\":8257538,\"global_name\":\"Name\",\"avatar_decoration_data\":null,\"collectibles\":null,\"display_name_styles\":null,\"banner_color\":\"#7e0002\",\"clan\":null,\"primary_guild\":null,\"mfa_enabled\":false,\"locale\":\"en-GB\",\"premium_type\":0,\"email\":\"test@test.com\",\"verified\":true}\n")
            });

        var result = await Sut.GetDiscordUserInfo(accessToken, _httpClient);

        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri("https://discord.com/api/users/@me") &&
                    req.Headers.Authorization != null &&
                    req.Headers.Authorization.Scheme == "Bearer" &&
                    req.Headers.Authorization.Parameter == accessToken
                ),
                ItExpr.IsAny<CancellationToken>());
        result.Should().NotBeNull();

        result.Should().BeEquivalentTo(new DiscordUser("123", "name", "test@test.com", "456", true, "0"));
    }

    [TestMethod]
    public async Task GetDiscordUserInfo_ReturnsNull_WhenApiIsNotSuccessful()
    {
        const string accessToken = "this-access-token";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri("https://discord.com/api/users/@me") &&
                    req.Headers.Authorization != null &&
                    req.Headers.Authorization.Scheme == "Bearer" &&
                    req.Headers.Authorization.Parameter == accessToken
                ),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

        var result = await Sut.GetDiscordUserInfo(accessToken, _httpClient);

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task GetDiscordUserInfo_ReturnsNull_WhenJsonExceptionThrown()
    {
        const string accessToken = "this-access-token";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri("https://discord.com/api/users/@me") &&
                    req.Headers.Authorization != null &&
                    req.Headers.Authorization.Scheme == "Bearer" &&
                    req.Headers.Authorization.Parameter == accessToken
                ),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid-json")
            });


        var act = () => Sut.GetDiscordUserInfo(accessToken, _httpClient);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ExchangeCodeForToken

    [TestMethod]
    public async Task ExchangeCodeForToken_ReturnsTokenResponse_WhenDiscordReturnsSuccess()
    {
        const string code = "test-auth-code";
        const string serverUri = "https://test-server.com";
        SetUpSecrets(
            discordClientId: "test-client-id",
            discordClientSecret: "test-client-secret",
            serverUri: serverUri
        );

        var expectedTokenResponse = new TokenResponse
        {
            AccessToken = "test-access-token",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            RefreshToken = "test-refresh-token",
            Scope = CommonMethods.GetRandomString()
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri("https://discord.com/api/oauth2/token")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expectedTokenResponse, jsonOptions))
            });

        var result = await Sut.ExchangeCodeForToken(code, _httpClient);

        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri("https://discord.com/api/oauth2/token") &&
                    ValidateTokenRequestContent(req, code, serverUri)),
                ItExpr.IsAny<CancellationToken>());

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTokenResponse);
    }

    [TestMethod]
    public async Task ExchangeCodeForToken_ReturnsNull_WhenDiscordReturnsError()
    {
        const string code = "invalid-code";
        SetUpSecrets(
            discordClientId: "test-client-id",
            discordClientSecret: "test-client-secret",
            serverUri: "https://test-server.com"
        );

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

        var result = await Sut.ExchangeCodeForToken(code, _httpClient);

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task ExchangeCodeForToken_SendsCorrectFormData_Always()
    {
        const string code = "test-code";
        const string serverUri = "https://my-server.com";
        const string clientId = "my-client-id";
        const string clientSecret = "my-client-secret";

        SetUpSecrets(
            discordClientId: clientId,
            discordClientSecret: clientSecret,
            serverUri: serverUri
        );

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });

        await Sut.ExchangeCodeForToken(code, _httpClient);

        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri("https://discord.com/api/oauth2/token") &&
                    ValidateTokenRequestContent(req, code, serverUri)),
                ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task ExchangeCodeForToken_HandlesJsonDeserializationError_ReturnsNull()
    {
        const string code = "test-code";
        SetUpSecrets(
            discordClientId: "test-client-id",
            discordClientSecret: "test-client-secret",
            serverUri: "https://test-server.com"
        );

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid-json")
            });

        var act = () => Sut.ExchangeCodeForToken(code, _httpClient);
        await act.Should().NotThrowAsync();
    }

    [TestMethod]
    public async Task ExchangeCodeForToken_UsesCorrectRedirectUri_Always()
    {
        const string code = "test-code";
        const string serverUri = "https://custom-server.com";
        SetUpSecrets(serverUri: serverUri);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });

        await Sut.ExchangeCodeForToken(code, _httpClient);

        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    ValidateRedirectUri(req, serverUri + "/api/discord/auth/callback")),
                ItExpr.IsAny<CancellationToken>());
    }

    private static bool ValidateTokenRequestContent(HttpRequestMessage request, string expectedCode, string serverUri)
    {
        if (request.Content is not FormUrlEncodedContent) return false;

        var contentTask = request.Content.ReadAsStringAsync();
        contentTask.Wait();
        var content = contentTask.Result;

        return content.Contains($"code={expectedCode}") &&
               content.Contains("grant_type=authorization_code") &&
               content.Contains($"redirect_uri={Uri.EscapeDataString(serverUri + "/api/discord/auth/callback")}");
    }

    private static bool ValidateRedirectUri(HttpRequestMessage request, string expectedRedirectUri)
    {
        if (request.Content is not FormUrlEncodedContent) return false;

        var contentTask = request.Content.ReadAsStringAsync();
        contentTask.Wait();
        var content = contentTask.Result;

        return content.Contains($"redirect_uri={Uri.EscapeDataString(expectedRedirectUri)}");
    }

    #endregion
}