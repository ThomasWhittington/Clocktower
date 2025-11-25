namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordChannel
{
    string Name { get; }
    Task DeleteAsync();
}