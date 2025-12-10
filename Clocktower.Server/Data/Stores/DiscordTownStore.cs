using System.Collections.Concurrent;

namespace Clocktower.Server.Data.Stores;

public class DiscordTownStore : IDiscordTownStore
{
    private readonly ConcurrentDictionary<string, DiscordTown> _store = new();

    public void Clear() => _store.Clear();

    public DiscordTown? Get(string? guildId)
    {
        if (guildId is null) return null;
        return _store.TryGetValue(guildId, out var state) ? state : null;
    }

    public DiscordTown? Get(ulong? guildId) => Get(guildId.ToString());

    public bool Remove(string guildId) => _store.TryRemove(guildId, out _);
    public bool Remove(ulong guildId) => Remove(guildId.ToString());

    public bool Set(string guildId, DiscordTown state, bool force = false)
    {
        var currentValue = Get(guildId);
        if (currentValue is not null && !force) return false;
        _store[guildId] = state;
        return true;
    }

    public bool Set(ulong guildId, DiscordTown state, bool force = false) => Set(guildId.ToString(), state, force);

    public bool TryUpdate(string guildId, Func<DiscordTown, DiscordTown> updateFn)
    {
        if (!_store.TryGetValue(guildId, out var existing)) return false;

        _store[guildId] = updateFn(existing);
        return true;
    }

    public bool TryUpdate(ulong guildId, Func<DiscordTown, DiscordTown> updateFn) => TryUpdate(guildId.ToString(), updateFn);

    public DiscordTown? GetTownByUser(string userId)
    {
        return _store.Values
            .FirstOrDefault(town => 
                town.ChannelCategories
                    .SelectMany(category => category.Channels)
                    .SelectMany(channel => channel.Occupants)
                    .Any(occupant => occupant.Id == userId));
    }
}