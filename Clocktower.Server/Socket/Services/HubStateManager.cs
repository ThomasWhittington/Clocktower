using Clocktower.Server.Common.Services;
using Clocktower.Server.Data.Extensions;

namespace Clocktower.Server.Socket.Services;

public class HubStateManager(IGameStateStore gameStateStore, ITownOccupancyStore townOccupancyStore, IJwtWriter jwtWriter) : IHubStateManager
{
    public SessionSyncState? GetState(string gameId, string userId)
    {
        var currentGameState = gameStateStore.Get(gameId);
        var gameUser = currentGameState?.GetUser(userId);
        if (currentGameState is null || gameUser is null) return null;

        var currentState = new SessionSyncState
        {
            GameTime = currentGameState.GameTime,
            Jwt = jwtWriter.GetJwtToken(gameUser),
            TownOccupancy = townOccupancyStore.Get(currentGameState.GuildId)
        };
        return currentState;
    }
}

public interface IHubStateManager
{
    SessionSyncState? GetState(string gameId, string userId);
}