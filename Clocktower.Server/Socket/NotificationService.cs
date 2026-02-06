using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public class NotificationService(IHubContext<NotificationHub, INotificationClient> hub) : INotificationService
{
    public Task SendBulkDiscordTownUpdates(IEnumerable<UserNotification> notifications) => Task.WhenAll(notifications.Select(n => hub.Clients.User(n.UserId).DiscordTownUpdated(n.Town)));
    public Task SendUserVoiceStateToGroup(string gameId, string userId, bool inVoice, VoiceState voiceState) => hub.Clients.Group(GetGameGroupName(gameId)).UserVoiceStateChanged(userId, inVoice, voiceState);
    public Task SendTownTimeToGroup(string gameId, GameTime gameTime) => hub.Clients.Group(GetGameGroupName(gameId)).TownTimeChanged(gameId, (int)gameTime);
    public Task PingUser(string targetUserId, string message) => hub.Clients.User(targetUserId).PingUser(message);
    public Task SendTimerUpdateToGroup(string gameId, TimerState timer) => hub.Clients.Group(GetGameGroupName(gameId)).TimerUpdated(timer);
    public Task SendScriptToGroup(string gameId, Script? script) => hub.Clients.Group(GetGameGroupName(gameId)).ScriptUpdated(gameId, script);
    public Task SendNominationSessionUpdateToGroup(string gameId, NominationSession? session) => hub.Clients.Group(GetGameGroupName(gameId)).NominationUpdate(gameId, session);
    public Task SendTalkRequestsUpdateToGroup(string gameId, IEnumerable<TalkRequest>? talkRequests) => hub.Clients.Group(GetGameGroupName(gameId)).TalkRequestsUpdate(gameId, talkRequests);

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}