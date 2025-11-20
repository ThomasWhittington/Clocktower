using Clocktower.Server.Common.Services;
using Clocktower.Server.Data.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public sealed class DiscordNotificationHub(IGameStateStore gameStateStore, IJwtWriter jwtWriter) : Hub<IDiscordNotificationClient>
{
    [UsedImplicitly]
    public async Task<SessionSyncState?> JoinGameGroup(string gameId, string userId)
    {
        var currentGameState = gameStateStore.Get(gameId);
        var gameUser = currentGameState?.GetUser(userId);
        if (gameUser is null) return null;
        var currentState = new SessionSyncState
        {
            GameTime = currentGameState?.GameTime ?? GameTime.Unknown,
            Jwt = jwtWriter.GetJwtToken(gameUser)
        };
        
        await Groups.AddToGroupAsync(Context.ConnectionId, GetGameGroupName(gameId));

        return currentState;
    }

    [UsedImplicitly]
    public Task LeaveGameGroup(string gameId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGameGroupName(gameId));

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}