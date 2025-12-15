namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordUser
{
    string Id { get; }
    string GlobalName { get; }
    string DisplayAvatarUrl { get; }

    IDiscordGuildUser? GetGuildUser();

    Task<IDiscordDmChannel?> CreateDmChannelAsync();
    GameUser AsGameUser();
}