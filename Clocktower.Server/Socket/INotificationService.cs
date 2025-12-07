namespace Clocktower.Server.Socket;

public interface INotificationService
{
    Task BroadcastTownOccupancyUpdate(string gameId, TownOccupants occupants);
    Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice, MutedState mutedState);
    Task BroadcastTownTime(string gameId, GameTime gameTime);
    Task PingUser(string targetUserId, string message);
}