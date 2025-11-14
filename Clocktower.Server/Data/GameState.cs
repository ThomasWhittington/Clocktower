namespace Clocktower.Server.Data;

public class GameState
{
    public string Id { get; set; }
    public string GuildId { get; set; }
    public List<GameUser> Players { get; set; } = [];
    public int MaxPlayers { get; set; } = 0;

    public GameUser CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }

    public bool IsFull => MaxPlayers != 0 && Players.Count >= MaxPlayers;
}