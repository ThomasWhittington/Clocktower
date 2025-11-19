namespace Clocktower.Server.Data.Stores;

public interface IGameStateStore
{
    void Clear();
    GameState? Get(string gameId);
    bool Set(string gameId, GameState state);
    bool Remove(string gameId);
    bool TryUpdate(string gameId, Func<GameState, GameState> updateFunction);
    IEnumerable<GameState> GetGuildGames(string guildId);
    IEnumerable<GameState> GetGuildGames(ulong guildId);
    IEnumerable<GameState> GetUserGames(string userId);
    IEnumerable<GameState> GetAll();
}