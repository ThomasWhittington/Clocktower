namespace Clocktower.Server.Data.Stores;

public interface IDiscordTownStore
{
    void Clear();
    DiscordTown? Get(string? guildId);
    bool Remove(string guildId);
    bool Set(string guildId, DiscordTown state, bool force = false);
    bool TryUpdate(string guildId, Func<DiscordTown, DiscordTown> updateFn);
    DiscordTown? GetTownByUser(string userId);
}