namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordUser
{
    ulong Id { get; }
    string GlobalName { get; }
    string DisplayAvatarUrl { get; }

    Task<IDiscordDmChannel?> CreateDmChannelAsync();
}