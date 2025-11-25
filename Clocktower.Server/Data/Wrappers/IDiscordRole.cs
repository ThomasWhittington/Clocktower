namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordRole
{
    string Name { get; }
    ulong Id { get; }
    Task DeleteAsync();
}