using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Clocktower.Server.Discord.Auth.Services;

public class DiscordAuthApiService(IOptions<Secrets> secretsOptions) : IDiscordAuthApiService
{
    private readonly Secrets _secrets = secretsOptions.Value;

    private readonly JsonSerializerOptions _deserializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<TokenResponse?> ExchangeCodeForToken(string code, HttpClient httpClient)
    {
        var redirectUri = _secrets.ServerUri + "/api/discord/auth/callback";
        var tokenRequest = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("client_id", _secrets.DiscordClientId),
            new KeyValuePair<string, string>("client_secret", _secrets.DiscordClientSecret),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", redirectUri)
        ]);

        var response = await httpClient.PostAsync("https://discord.com/api/oauth2/token", tokenRequest);

        if (!response.IsSuccessStatusCode)
        {
            Debugger.Break();
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TokenResponse>(json, _deserializationOptions);
    }

    public async Task<DiscordUser?> GetDiscordUserInfo(string accessToken, HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync("https://discord.com/api/users/@me");

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiscordUser>(json, _deserializationOptions);
    }
}