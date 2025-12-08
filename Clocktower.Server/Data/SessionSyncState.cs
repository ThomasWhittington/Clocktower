namespace Clocktower.Server.Data;

public class SessionSyncState
{
    public GameTime GameTime { get; set; }
    public string Jwt { get; set; }
    public TownOccupants? TownOccupancy { get; set; }
}