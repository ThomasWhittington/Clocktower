namespace Clocktower.Server.Data.Stores;

public interface IDiscordTownStore
{
    void Clear();
    DiscordTown? Get(string? guildId);
    DiscordTown? Get(ulong? guildId);
    bool Remove(string guildId);
    bool Remove(ulong guildId);
    bool Set(string guildId, DiscordTown state, bool force = false);
    bool Set(ulong guildId, DiscordTown state, bool force = false);
    bool TryUpdate(string guildId, Func<DiscordTown, DiscordTown> updateFn);
    bool TryUpdate(ulong guildId, Func<DiscordTown, DiscordTown> updateFn);
    DiscordTown? GetTownByUser(string userId);
}