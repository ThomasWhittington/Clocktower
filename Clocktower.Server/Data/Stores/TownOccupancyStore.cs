using System.Collections.Concurrent;

namespace Clocktower.Server.Data.Stores;

public class TownOccupancyStore : ITownOccupancyStore
{
    private readonly ConcurrentDictionary<string, TownOccupants> _store = new();

    public void Clear() => _store.Clear();

    public TownOccupants? Get(string? guildId)
    {
        if (guildId is null) return null;
        return _store.TryGetValue(guildId, out var state) ? state : null;
    }

    public TownOccupants? Get(ulong? guildId)
    {
        if (guildId is null) return null;
        return Get(guildId.ToString());
    }

    public bool Remove(string guildId) => _store.TryRemove(guildId, out _);
    public bool Remove(ulong guildId) => Remove(guildId.ToString());

    public bool Set(string guildId, TownOccupants state, bool force = false)
    {
        var currentValue = Get(guildId);
        if (currentValue is not null && !force) return false;
        _store[guildId] = state;
        return true;
    }

    public bool Set(ulong guildId, TownOccupants state, bool force = false) => Set(guildId.ToString(), state, force);

    public bool TryUpdate(string guildId, Func<TownOccupants, TownOccupants> updateFn)
    {
        if (!_store.TryGetValue(guildId, out var existing)) return false;

        _store[guildId] = updateFn(existing);
        return true;
    }

    public bool TryUpdate(ulong guildId, Func<TownOccupants, TownOccupants> updateFn) => TryUpdate(guildId.ToString(), updateFn);
}