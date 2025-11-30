namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordVoiceChannel
{
    ulong Id { get; }
    string Name { get; }
    IEnumerable<IDiscordGuildUser> ConnectedUsers { get; }
    ulong? CategoryId { get; }
    int Position { get; }
    IDiscordGuild Guild { get; }
}