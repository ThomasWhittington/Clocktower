namespace Clocktower.Server.Socket;

public interface INotificationService
{
    /// <summary>
/// Broadcasts an update to the Discord representation for the specified game/town.
/// </summary>
/// <param name="gameId">Identifier of the game/town whose Discord update should be sent.</param>
Task BroadcastDiscordTownUpdate(string gameId);
    /// <summary>
/// Broadcasts a user's voice state change for the specified game.
/// </summary>
/// <param name="gameId">Identifier of the game/town context for the notification.</param>
/// <param name="userId">Identifier of the user whose voice state changed.</param>
/// <param name="inVoice">`true` if the user is currently connected to a voice channel, `false` otherwise.</param>
/// <param name="voiceState">Additional voice state details (for example: mute/deafen status and channel information).</param>
Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice, VoiceState voiceState);
    /// <summary>
/// Broadcasts the current game/town time update to all subscribers for the specified game.
/// </summary>
/// <param name="gameId">Identifier of the game/town whose time is being broadcast.</param>
/// <param name="gameTime">The current or updated game time information to broadcast.</param>
Task BroadcastTownTime(string gameId, GameTime gameTime);
    /// <summary>
/// Sends a direct notification message to a specific user identified by their ID.
/// </summary>
/// <param name="targetUserId">The identifier of the user to receive the ping.</param>
/// <param name="message">The text content of the notification to send.</param>
Task PingUser(string targetUserId, string message);
    Task BroadcastTimerUpdate(string gameId, TimerState timer);
}