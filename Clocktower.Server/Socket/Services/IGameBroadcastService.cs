namespace Clocktower.Server.Socket.Services;

public interface IGameBroadcastService
{
    Task BroadcastDiscordTownUpdate(string gameId);
    Task BroadcastTimeUpdate(string gameId, GameTime gameTime);
    Task BroadcastScriptUpdate(string gameId, Script script);
    Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice, VoiceState voiceState);
    Task BroadcastTimerUpdate(string gameId, TimerState timer);
    Task BroadcastNominationSessionUpdate(string gameId, NominationSession? nominationSession);
    Task BroadcastTalkRequestsUpdate(string gameId, IEnumerable<TalkRequest>? talkRequests);
    Task BroadcastPlayAudio(string gameId, AudioEvent audio);
}