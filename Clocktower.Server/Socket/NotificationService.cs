using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public class NotificationService(IHubContext<DiscordNotificationHub, IDiscordNotificationClient> hub)
    : INotificationService
{
    public Task BroadcastTownOccupancyUpdate(string gameId, TownOccupants occupants) => hub.Clients.Group(GetGameGroupName(gameId)).TownOccupancyUpdated(occupants);


    public Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice) => hub.Clients.Group(GetGameGroupName(gameId)).UserVoiceStateChanged(userId, inVoice);
    public Task BroadcastTownTime(GameTime gameTime) => hub.Clients.All.TownTimeChanged((int)gameTime);
    public Task BroadcastTownTime(string gameId, GameTime gameTime) => hub.Clients.Group(GetGameGroupName(gameId)).TownTimeChanged((int)gameTime);

    public Task NotifyUserVoiceStateChanged(string targetUserId, bool inVoice) => hub.Clients.User(targetUserId).UserVoiceStateChanged(targetUserId, inVoice);
    public Task NotifyUserTownOccupancy(string targetUserId, TownOccupants occupants) => hub.Clients.User(targetUserId).TownOccupancyUpdated(occupants);

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}