using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public interface IDiscordNotificationClient
{
    Task TownOccupancyUpdated(TownOccupants townOccupants);
    Task UserVoiceStateChanged(string userId, bool isInVoice);
}

public sealed class DiscordNotificationHub : Hub<IDiscordNotificationClient>;