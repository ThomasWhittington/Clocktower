namespace Clocktower.Server.Data;

public class SessionSyncState
{
    public GameTime GameTime { get; set; }
    public string Jwt { get; set; }
    public DiscordTownDto? DiscordTown { get; set; }
}