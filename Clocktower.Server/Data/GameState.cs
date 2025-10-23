namespace Clocktower.Server.Data;

public class GameState
{
    public string GameId { get; set; }
    public List<Player> Players { get; set; } = [];
}