using Discord.WebSocket;

namespace Clocktower.Server.Discord.Services;

public interface IDiscordBotService:IHostedService
{
    public DiscordSocketClient Client { get;  }
}