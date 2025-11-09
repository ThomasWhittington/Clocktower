using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Clocktower.Server.Discord.Auth.Services;

public class DiscordAuthService(Secrets secrets, IMemoryCache cache) : IDiscordAuthService
{
    private readonly JsonSerializerOptions _deserializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public (bool success, string url, string message) GetAuthorizationUrl()
    {
        const string scopes = "identify guilds";

        if (string.IsNullOrEmpty(secrets.DiscordClientId) || string.IsNullOrEmpty(secrets.DiscordRedirectUri))
        {
            return (false, string.Empty, "Discord OAuth not properly configured");
        }

        var authorizationUrl = $"https://discord.com/api/oauth2/authorize" +
                               $"?client_id={secrets.DiscordClientId}" +
                               $"&redirect_uri={Uri.EscapeDataString(secrets.DiscordRedirectUri)}" +
                               $"&response_type=code" +
                               $"&scope={Uri.EscapeDataString(scopes)}";

        return (true, authorizationUrl, "Authorization Url generated");
    }

    public async Task<string> HandleCallback(string? error, string? code)
    {
        const string frontendUrl = "http://localhost:5173/auth/callback?key=";
        const string errorUrl = "http://localhost:5173/login?error=";

        if (!string.IsNullOrEmpty(error)) return errorUrl + Uri.EscapeDataString($"Discord OAuth error: {error}");
        if (string.IsNullOrEmpty(code)) return errorUrl + Uri.EscapeDataString("Authorization code not received");

        try
        {
            using var httpClient = new HttpClient();

            var tokenResponse = await ExchangeCodeForToken(code, httpClient);
            if (tokenResponse == null) return errorUrl + Uri.EscapeDataString("Failed to exchange code for token");

            var userInfo = await GetDiscordUserInfo(tokenResponse.AccessToken, httpClient);
            if (userInfo == null) return errorUrl + Uri.EscapeDataString("Failed to get user information");

            var avatarUrl = userInfo.Avatar != null
                ? $"https://cdn.discordapp.com/avatars/{userInfo.Id}/{userInfo.Avatar}.png"
                : "https://cdn.discordapp.com/embed/avatars/0.png";

            var response = new MiniUser(userInfo.Id, userInfo.Username, avatarUrl);
            var tempKey = Guid.NewGuid().ToString();
            cache.Set($"auth_data_{tempKey}", response, TimeSpan.FromMinutes(5));

            return frontendUrl + tempKey;
        }
        catch (Exception ex)
        {
            return errorUrl + Uri.EscapeDataString($"Authentication failed: {ex.Message}");
        }
    }

    public string HandleBotCallback(string? error, string? code, string? guildId)
    {
        const string frontendUrl = "http://localhost:5173/auth/bot-callback?guild_id=";
        const string errorUrl = "http://localhost:5173/login?error=";

        if (!string.IsNullOrEmpty(error)) return errorUrl + Uri.EscapeDataString($"Discord OAuth error: {error}");
        if (string.IsNullOrEmpty(code)) return errorUrl + Uri.EscapeDataString("Authorization code not received");
        if (string.IsNullOrEmpty(guildId)) return errorUrl + Uri.EscapeDataString("No guildId received");

        return frontendUrl + Uri.EscapeDataString(guildId);
    }

    public (bool success, string url, string message) GetAddBotUrl()
    {
        if (string.IsNullOrEmpty(secrets.DiscordClientId)) return (false, string.Empty, "Discord OAuth not properly configured");

        var permissionsInt = 8;

        var authorizationUrl = $"https://discord.com/oauth2/authorize" +
                               $"?client_id={secrets.DiscordClientId}" +
                               $"&permissions={permissionsInt}" +
                               $"&integration_type=0" +
                               $"&scope=bot" +
                               $"&response_type=code" +
                               $"&redirect_uri={Uri.EscapeDataString(secrets.DiscordBotRedirectUri)}";

        return (true, authorizationUrl, "Bot addition Url generated");
    }

    public MiniUser? GetAuthData(string key)
    {
        if (cache.TryGetValue($"auth_data_{key}", out var userData) && userData is MiniUser response)
        {
            cache.Remove($"auth_data_{key}");
            return response;
        }

        return null;
    }

    private async Task<TokenResponse?> ExchangeCodeForToken(string code, HttpClient httpClient)
    {
        var tokenRequest = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id", secrets.DiscordClientId),
            new KeyValuePair<string, string>("client_secret", secrets.DiscordClientSecret),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", secrets.DiscordRedirectUri)
        ]);

        var response = await httpClient.PostAsync("https://discord.com/api/oauth2/token", tokenRequest);

        if (!response.IsSuccessStatusCode)
        {
            Debugger.Break();
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TokenResponse>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
    }

    private async Task<DiscordUser?> GetDiscordUserInfo(string accessToken, HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync("https://discord.com/api/users/@me");

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiscordUser>(json, _deserializationOptions);
    }

    private sealed record TokenResponse(
        string AccessToken,
        string TokenType,
        int ExpiresIn,
        string RefreshToken,
        string Scope
    );

    private sealed record DiscordUser(
        string Id,
        string Username,
        string? Email,
        string? Avatar,
        bool? Verified,
        string Discriminator
    );
}