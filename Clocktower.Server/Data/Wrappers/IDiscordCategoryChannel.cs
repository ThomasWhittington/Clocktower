namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordCategoryChannel
{
    ulong Id { get; }
    string Name { get; }
    IEnumerable<IDiscordChannel> Channels { get; }
    Task DeleteAsync();
}