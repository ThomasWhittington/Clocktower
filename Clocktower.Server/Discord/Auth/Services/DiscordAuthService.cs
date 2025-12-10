using Clocktower.Server.Common.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Clocktower.Server.Discord.Auth.Services;

public class DiscordAuthService(IOptions<Secrets> secretsOptions, IJwtWriter jwtWriter, IMemoryCache cache, IHttpClientFactory httpClientFactory, IDiscordAuthApiService discordAuthApiService, IIdGenerator idGenerator) : IDiscordAuthService
{
    private readonly Secrets _secrets = secretsOptions.Value;


    public (bool success, string url, string message) GetAuthorizationUrl()
    {
        const string scopes = "identify guilds";

        if (string.IsNullOrEmpty(_secrets.DiscordClientId) || string.IsNullOrEmpty(_secrets.ServerUri))
        {
            return (false, string.Empty, "Discord OAuth not properly configured");
        }

        var redirectUri = _secrets.ServerUri + "/api/discord/auth/callback";

        var authorizationUrl = $"https://discord.com/api/oauth2/authorize" +
                               $"?client_id={_secrets.DiscordClientId}" +
                               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                               $"&response_type=code" +
                               $"&scope={Uri.EscapeDataString(scopes)}";

        return (true, authorizationUrl, "Authorization Url generated");
    }

    public async Task<string> HandleCallback(string? error, string? code)
    {
        string frontendUrl = _secrets.FeUri + "/auth/callback?key=";
        string errorUrl = _secrets.FeUri + "/login?error=";

        if (!string.IsNullOrEmpty(error)) return errorUrl + Uri.EscapeDataString($"Discord OAuth error: {error}");
        if (string.IsNullOrEmpty(code)) return errorUrl + Uri.EscapeDataString("Authorization code not received");

        try
        {
            using var httpClient = httpClientFactory.CreateClient();

            var tokenResponse = await discordAuthApiService.ExchangeCodeForToken(code, httpClient);
            if (tokenResponse == null) return errorUrl + Uri.EscapeDataString("Failed to exchange code for token");

            var userInfo = await discordAuthApiService.GetDiscordUserInfo(tokenResponse.AccessToken, httpClient);
            if (userInfo == null) return errorUrl + Uri.EscapeDataString("Failed to get user information");

            var townUser = userInfo.AsTownUser();
            var jwt = jwtWriter.GetJwtToken(townUser);
            var userAuthData = new UserAuthData(townUser, jwt);
            var tempKey = idGenerator.GenerateId();
            cache.Set($"auth_data_{tempKey}", userAuthData, TimeSpan.FromMinutes(5));

            return frontendUrl + tempKey;
        }
        catch (Exception ex)
        {
            return errorUrl + Uri.EscapeDataString($"Authentication failed: {ex.Message}");
        }
    }

    public string HandleBotCallback(string? error, string? code, string? guildId)
    {
        string frontendUrl = _secrets.FeUri + "/auth/bot-callback?guild_id=";
        string errorUrl = _secrets.FeUri + "/login?error=";

        if (!string.IsNullOrEmpty(error)) return errorUrl + Uri.EscapeDataString($"Discord OAuth error: {error}");
        if (string.IsNullOrEmpty(code)) return errorUrl + Uri.EscapeDataString("Authorization code not received");
        if (string.IsNullOrEmpty(guildId)) return errorUrl + Uri.EscapeDataString("No guildId received");

        return frontendUrl + Uri.EscapeDataString(guildId);
    }

    public (bool success, string url, string message) GetAddBotUrl()
    {
        if (string.IsNullOrEmpty(_secrets.DiscordClientId) || string.IsNullOrEmpty(_secrets.ServerUri))
            return (false, string.Empty, "Discord OAuth not properly configured");

        const int permissionsInt = 8;
        var redirectUri = _secrets.ServerUri + "/api/discord/auth/bot-callback";
        var authorizationUrl = $"https://discord.com/oauth2/authorize" +
                               $"?client_id={_secrets.DiscordClientId}" +
                               $"&permissions={permissionsInt}" +
                               $"&integration_type=0" +
                               $"&scope=bot" +
                               $"&response_type=code" +
                               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";

        return (true, authorizationUrl, "Bot addition Url generated");
    }

    public UserAuthData? GetAuthData(string key)
    {
        if (cache.TryGetValue($"auth_data_{key}", out var userData) && userData is UserAuthData response)
        {
            cache.Remove($"auth_data_{key}");
            return response;
        }

        return null;
    }
}