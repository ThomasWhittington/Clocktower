namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordRole
{
    string Id { get; }
    string Name { get; }
    Task DeleteAsync();
}