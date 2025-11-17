namespace Clocktower.Server.Socket;

public interface INotificationService
{
    Task BroadcastTownOccupancyUpdate(string gameId, TownOccupants occupants);
    Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice);
    Task BroadcastTownTime(string gameId, GameTime gameTime);

    Task PingUser(string targetUserId, string message);
    Task NotifyUserVoiceStateChanged(string targetUserId, bool inVoice);
    Task NotifyUserTownOccupancy(string targetUserId, TownOccupants occupants);
}