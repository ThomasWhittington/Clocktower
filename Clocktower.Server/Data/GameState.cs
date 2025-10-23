namespace Clocktower.Server.Data;

public class GameState
{
    public string GameId { get; set; }
    public List<Player> Players { get; set; } = [];
    public int MaxPlayers { get; set; } = 0;

    public bool IsFull => MaxPlayers != 0 && Players.Count >= MaxPlayers;
}