using Discord;
using Discord.WebSocket;

namespace Clocktower.Server.Discord.Services;

public interface IDiscordBotService:IHostedService
{
    public DiscordSocketClient Client { get;  }
    public IUser GetUser(ulong userId);
}