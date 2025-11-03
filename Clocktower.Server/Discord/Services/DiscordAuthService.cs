using System.Diagnostics;
using System.Text.Json;
using Clocktower.Server.Common;

namespace Clocktower.Server.Discord.Services;

public class DiscordAuthService(Secrets secrets)
{
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

    public async Task<(bool success, AuthResult? authResult, string message)> HandleCallback(string? error, string? code)
    {
        if (!string.IsNullOrEmpty(error)) return (false, null, $"Discord OAuth error: {error}");
        if (string.IsNullOrEmpty(code)) return (false, null, "Authorization code not received");

        try
        {
            using var httpClient = new HttpClient();

            var tokenResponse = await ExchangeCodeForToken(code, httpClient);
            if (tokenResponse == null)
            {
                return (false, null, "Failed to exchange code for token");
            }

            var userInfo = await GetDiscordUserInfo(tokenResponse.AccessToken, httpClient);
            if (userInfo == null)
            {
                return (false, null, "Failed to get user information");
            }

            // Here you would typically:
            // 1. Create or update user in your database
            // 2. Generate JWT token for your application
            // 3. Set authentication cookies

            var result = new AuthResult(true, userInfo, tokenResponse.AccessToken, tokenResponse.RefreshToken);

            return (true, result, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, null, $"Authentication failed: {ex.Message}");
        }
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

    private static async Task<DiscordUser?> GetDiscordUserInfo(string accessToken, HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);

        var response = await httpClient.GetAsync("https://discord.com/api/users/@me");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiscordUser>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
    }

    public record TokenResponse(
        string AccessToken,
        string TokenType,
        int ExpiresIn,
        string RefreshToken,
        string Scope
    );

    public record DiscordUser(
        string Id,
        string Username,
        string? Email,
        string? Avatar,
        bool? Verified,
        string Discriminator
    );

    public record AuthResult(
        bool Success,
        DiscordUser? User,
        string? AccessToken,
        string? RefreshToken,
        string? Error = null
    );
}