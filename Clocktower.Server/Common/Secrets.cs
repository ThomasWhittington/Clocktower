namespace Clocktower.Server.Common;

public class Secrets
{
    [UsedImplicitly] public string DiscordBotToken { get; set; }
    [UsedImplicitly] public string DiscordClientId { get; set; }
    [UsedImplicitly] public string DiscordClientSecret { get; set; }
    [UsedImplicitly] public string ServerUri { get; set; }
    [UsedImplicitly] public string FeUri { get; set; }
}