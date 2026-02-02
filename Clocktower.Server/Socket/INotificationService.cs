namespace Clocktower.Server.Socket;

public interface INotificationService
{
    Task SendBulkDiscordTownUpdates(IEnumerable<UserNotification> notifications);
    Task SendUserVoiceStateToGroup(string gameId, string userId, bool inVoice, VoiceState voiceState);
    Task SendTownTimeToGroup(string gameId, GameTime gameTime);
    Task PingUser(string targetUserId, string message);
    Task SendTimerUpdateToGroup(string gameId, TimerState timer);
    Task SendScriptToGroup(string gameId, Script? script);
    Task SendNominationSessionUpdateToGroup(string gameId, NominationSession? session);
}