using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public class NotificationService(IHubContext<DiscordNotificationHub, IDiscordNotificationClient> hub)
    : INotificationService
{
    public Task BroadcastTownOccupancyUpdate(TownOccupants occupants) => hub.Clients.All.TownOccupancyUpdated(occupants);
    public Task BroadcastUserVoiceStateChanged(string userId, bool inVoice) => hub.Clients.All.UserVoiceStateChanged(userId, inVoice);
    public Task BroadcastTownTime(GameTime gameTime) => hub.Clients.All.TownTimeChanged((int)gameTime);
}