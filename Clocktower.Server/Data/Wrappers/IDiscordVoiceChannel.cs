namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordVoiceChannel
{
    string Id { get; }
    string Name { get; }
    IEnumerable<IDiscordGuildUser> ConnectedUsers { get; }
    string? CategoryId { get; }
    int Position { get; }
    IDiscordGuild Guild { get; }
}