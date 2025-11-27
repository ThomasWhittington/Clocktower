namespace Clocktower.Server.Discord.Auth.Services;

public interface IDiscordAuthApiService
{
    Task<TokenResponse?> ExchangeCodeForToken(string code, HttpClient httpClient);
    Task<DiscordUser?> GetDiscordUserInfo(string accessToken, HttpClient httpClient);
}