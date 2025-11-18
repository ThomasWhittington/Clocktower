using System.Collections.Concurrent;

namespace Clocktower.Server.Data.Stores;

public static class TownOccupancyStore
{
    private static readonly ConcurrentDictionary<string, TownOccupants> Store = new();

    public static void Clear() => Store.Clear();

    public static TownOccupants? Get(string guildId) =>
        Store.TryGetValue(guildId, out var state) ? state : null;

    public static TownOccupants? Get(ulong guildId) => Get(guildId.ToString());

    public static bool Remove(string guildId) => Store.TryRemove(guildId, out _);
    public static bool Remove(ulong guildId) => Remove(guildId.ToString());

    public static bool Set(string guildId, TownOccupants state)
    {
        var currentValue = Get(guildId);
        if (currentValue is not null) return false;
        Store[guildId] = state;
        return true;
    }

    public static bool Set(ulong guildId, TownOccupants state) => Set(guildId.ToString(), state);

    public static bool TryUpdate(string guildId, Func<TownOccupants, TownOccupants> updateFn)
    {
        if (!Store.TryGetValue(guildId, out var existing)) return false;

        Store[guildId] = updateFn(existing);
        return true;
    }

    public static bool TryUpdate(ulong guildId, Func<TownOccupants, TownOccupants> updateFn) => TryUpdate(guildId.ToString(), updateFn);
}