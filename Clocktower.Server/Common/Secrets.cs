namespace Clocktower.Server.Common;

public class Secrets
{
    [UsedImplicitly] public required string DiscordBotToken { get; set; }
    [UsedImplicitly] public required string DiscordClientId { get; set; }
    [UsedImplicitly] public required string DiscordClientSecret { get; set; }
    [UsedImplicitly] public required string ServerUri { get; set; }
    [UsedImplicitly] public required string FeUri { get; set; }
    [UsedImplicitly] public required JwtSecrets Jwt { get; set; }

    public (bool success, string message) HasAllSecrets()
    {
        if (string.IsNullOrWhiteSpace(DiscordBotToken)) return (false, $"Missing {nameof(DiscordBotToken)}");
        if (string.IsNullOrWhiteSpace(DiscordClientId)) return (false, $"Missing {nameof(DiscordClientId)}");
        if (string.IsNullOrWhiteSpace(DiscordClientSecret)) return (false, $"Missing {nameof(DiscordClientSecret)}");
        if (string.IsNullOrWhiteSpace(ServerUri)) return (false, $"Missing {nameof(ServerUri)}");
        if (string.IsNullOrWhiteSpace(FeUri)) return (false, $"Missing {nameof(FeUri)}");
        var jwtResult = Jwt.HasAllSecrets();
        if (!jwtResult.success)
        {
            return (false, jwtResult.message);
        }
        return (true, string.Empty);
    }
}