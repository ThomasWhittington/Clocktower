using System.Collections.Concurrent;

namespace Clocktower.Server.Data.Stores;

public class GameStateStore : IGameStateStore
{
    private static readonly ConcurrentDictionary<string, GameState> Store = new();

    public void Clear() => Store.Clear();

    public GameState? Get(string gameId) =>
        Store.TryGetValue(gameId, out var state) ? state : null;

    public bool Remove(string gameId) => Store.TryRemove(gameId, out _);

    public bool Set(string gameId, GameState state)
    {
        var currentValue = Get(gameId);
        if (currentValue is not null) return false;
        Store[gameId] = state;
        return true;
    }


    public bool TryUpdate(string gameId, Func<GameState, GameState> updateFunction)
    {
        if (!Store.TryGetValue(gameId, out var existing)) return false;

        Store[gameId] = updateFunction(existing);
        return true;
    }

    public IEnumerable<GameState> GetGuildGames(string guildId)
    {
        return GetAll().Where(game => game.GuildId == guildId);
    }

    public IEnumerable<GameState> GetGuildGames(ulong guildId) => GetGuildGames(guildId.ToString());

    public IEnumerable<GameState> GetUserGames(string userId)
    {
        return GetAll().Where(game => game.Users.Select(o => o.Id).Contains(userId));
    }

    public IEnumerable<GameState> GetAll() => Store.Values;
}