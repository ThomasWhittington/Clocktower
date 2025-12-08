using Clocktower.Server.Socket.Services;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public sealed class DiscordNotificationHub(IHubStateManager hubStateManager) : Hub<IDiscordNotificationClient>
{
    [UsedImplicitly]
    public async Task<SessionSyncState?> JoinGameGroup(string gameId, string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGameGroupName(gameId));

        var currentState = hubStateManager.GetState(gameId, userId);
        return currentState;
    }

    [UsedImplicitly]
    public Task LeaveGameGroup(string gameId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGameGroupName(gameId));

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}