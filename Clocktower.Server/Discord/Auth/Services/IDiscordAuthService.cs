namespace Clocktower.Server.Discord.Auth.Services;

public interface IDiscordAuthService
{
    (bool success, string url, string message) GetAuthorizationUrl();
    Task<string> HandleCallback(string? error, string? code);
    string HandleBotCallback(string? error, string? code, string? guildId);
    (bool success, string url, string message) GetAddBotUrl();
    UserAuthData? GetAuthData(string key);
}