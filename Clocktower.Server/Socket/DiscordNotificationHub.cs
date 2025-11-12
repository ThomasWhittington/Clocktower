using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public interface IDiscordNotificationClient
{
    Task TownOccupancyUpdated(TownOccupants townOccupants);
    Task UserVoiceStateChanged(string userId, bool isInVoice);
    Task TownTimeChanged(int gameTime);
}

public sealed class DiscordNotificationHub : Hub<IDiscordNotificationClient>;