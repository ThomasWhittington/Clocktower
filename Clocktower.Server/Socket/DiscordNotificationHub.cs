using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public sealed class DiscordNotificationHub : Hub<IDiscordNotificationClient>
{
    [UsedImplicitly]
    public async Task<SessionSyncState?> JoinGameGroup(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGameGroupName(gameId));
     
        var currentGameState = GameStateStore.Get(gameId);
        var currentState = new SessionSyncState
        {
            GameTime = currentGameState?.GameTime ?? GameTime.Unknown
        };

        return currentState;
    }

    [UsedImplicitly]
    public Task LeaveGameGroup(string gameId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGameGroupName(gameId));

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}

public interface IDiscordNotificationClient
{
    Task TownOccupancyUpdated(TownOccupants townOccupants);
    Task UserVoiceStateChanged(string userId, bool isInVoice);
    Task TownTimeChanged(int gameTime);
    Task PingUser(string message);
}