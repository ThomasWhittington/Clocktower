namespace Clocktower.Server.Data.Stores;

public interface IGameStateStore
{
    void Clear();
    GameState? Get(string gameId);
    bool Set(GameState state);
    bool Remove(string gameId);
    bool TryUpdate(string gameId, Func<GameState, GameState> updateFunction);
    IEnumerable<GameState> GetGuildGames(string guildId);
    IEnumerable<GameState> GetGuildGames(ulong guildId);
    IEnumerable<GameState> GetUserGames(string userId);
    IEnumerable<GameState> GetAll();

    void AddUserToGame(string gameId, GameUser gameUser);
    void SetTime(string gameId, GameTime gameTime);
    bool UpdateUser(string gameId, ulong userId, UserType? userType = null, bool? isPlaying = null);
}