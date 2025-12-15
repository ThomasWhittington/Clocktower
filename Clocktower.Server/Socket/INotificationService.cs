namespace Clocktower.Server.Socket;

public interface INotificationService
{
    Task BroadcastDiscordTownUpdate(string gameId, DiscordTown discordTown);
    Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice, VoiceState voiceState);
    Task BroadcastTownTime(string gameId, GameTime gameTime);
    Task PingUser(string targetUserId, string message);
    Task BroadcastTimerUpdate(string gameId, TimerState timer);
}