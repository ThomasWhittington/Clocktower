namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordDmChannel
{
    Task SendMessageAsync(string message);
}