using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public class NotificationService(IHubContext<DiscordNotificationHub, IDiscordNotificationClient> hub) : INotificationService
{
    public Task SendBulkDiscordTownUpdates(IEnumerable<UserNotification> notifications) => Task.WhenAll(notifications.Select(n => hub.Clients.User(n.UserId).DiscordTownUpdated(n.Town)));
    public Task SendUserVoiceStateToGroup(string gameId, string userId, bool inVoice, VoiceState voiceState) => hub.Clients.Group(GetGameGroupName(gameId)).UserVoiceStateChanged(userId, inVoice, voiceState);
    public Task SendTownTimeToGroup(string gameId, GameTime gameTime) => hub.Clients.Group(GetGameGroupName(gameId)).TownTimeChanged(gameId, (int)gameTime);
    public Task PingUser(string targetUserId, string message) => hub.Clients.User(targetUserId).PingUser(message);
    public Task SendTimerUpdateToGroup(string gameId, TimerState timer) => hub.Clients.Group(GetGameGroupName(gameId)).TimerUpdated(timer);

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}