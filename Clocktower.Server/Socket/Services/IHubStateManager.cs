namespace Clocktower.Server.Socket.Services;

public interface IHubStateManager
{
    SessionSyncState? GetState(string gameId, string userId);
}