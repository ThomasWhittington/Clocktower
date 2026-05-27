namespace Clocktower.Server.Socket;

public interface INotificationService
{
    Task SendBulkDiscordTownUpdates(IEnumerable<UserNotification> notifications);
    Task SendUserVoiceStateToGroup(string gameId, string userId, bool inVoice, VoiceState voiceState);
    Task SendTownTimeToGroup(string gameId, GameTime gameTime);
    Task PlayAudio(string gameId, AudioEvent audio);
    Task SendTimerUpdateToGroup(string gameId, TimerState timer);
    Task SendScriptToGroup(string gameId, Script? script);
    Task SendNominationSessionUpdateToGroup(string gameId, NominationSession? session);
    Task SendTalkRequestsUpdateToGroup(string gameId, IEnumerable<TalkRequest>? talkRequests);
}