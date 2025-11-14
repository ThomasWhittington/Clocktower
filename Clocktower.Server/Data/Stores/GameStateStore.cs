using System.Collections.Concurrent;

namespace Clocktower.Server.Data.Stores;

public class GameStateStore
{
    private static readonly ConcurrentDictionary<string, GameState> Store = new();

    public static void Clear() => Store.Clear();

    public static GameState? Get(string gameId) =>
        Store.TryGetValue(gameId, out var state) ? state : null;

    public static bool Remove(string gameId) => Store.TryRemove(gameId, out _);

    public static bool Set(string gameId, GameState state)
    {
        var currentValue = Get(gameId);
        if (currentValue is not null) return false;
        Store[gameId] = state;
        return true;
    }

    public static bool TryUpdate(string gameId, Func<GameState, GameState> updateFn)
    {
        if (!Store.TryGetValue(gameId, out var existing)) return false;

        Store[gameId] = updateFn(existing);
        return true;
    }

    public static IEnumerable<GameState> GetGames(string guildId)
    {
        return GetAll().Where(game => game.GuildId == guildId);
    }

    public static IEnumerable<GameState> GetAll()
    {
        return Store.Values;
    }

    public static IEnumerable<GameState> GetPlayerGames(string userId)
    {
        return GetAll().Where(game => game.Players.Select(o => o.Id).Contains(userId));
    }
}