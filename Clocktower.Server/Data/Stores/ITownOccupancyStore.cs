namespace Clocktower.Server.Data.Stores;

public interface ITownOccupancyStore
{
    void Clear();
    TownOccupants? Get(string guildId);
    TownOccupants? Get(ulong guildId);
    bool Remove(string guildId);
    bool Remove(ulong guildId);
    bool Set(string guildId, TownOccupants state);
    bool Set(ulong guildId, TownOccupants state);
    bool TryUpdate(string guildId, Func<TownOccupants, TownOccupants> updateFn);
    bool TryUpdate(ulong guildId, Func<TownOccupants, TownOccupants> updateFn);
}