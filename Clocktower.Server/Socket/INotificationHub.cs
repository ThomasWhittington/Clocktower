namespace Clocktower.Server.Socket;

public interface INotificationHub
{
    Task<SessionSyncState?> JoinGameGroup(string gameId, string userId, string? oldGameId = null);
    Task LeaveGameGroup(string gameId);
}