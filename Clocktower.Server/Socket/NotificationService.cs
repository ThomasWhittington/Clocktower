using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public class NotificationService(IHubContext<DiscordNotificationHub, IDiscordNotificationClient> hub)
    : INotificationService
{
    public Task BroadcastDiscordTownUpdate(string gameId, DiscordTown occupants) => hub.Clients.Group(GetGameGroupName(gameId)).DiscordTownUpdated(occupants);
    public Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice, VoiceState voiceState) => hub.Clients.Group(GetGameGroupName(gameId)).UserVoiceStateChanged(userId, inVoice, voiceState);
    public Task BroadcastTownTime(string gameId, GameTime gameTime) => hub.Clients.Group(GetGameGroupName(gameId)).TownTimeChanged((int)gameTime);
    public Task PingUser(string targetUserId, string message) => hub.Clients.User(targetUserId).PingUser(message);

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}