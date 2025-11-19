using Clocktower.Server.Data.Extensions;

namespace Clocktower.Server.Common.Api.Auth;

public class GameAuthorizationService(IGameStateStore gameStateStore) : IGameAuthorizationService
{
    public bool IsStoryTellerForGame(string userId, string gameId)
    {
        var gameState = gameStateStore.Get(gameId);
        return gameState is not null && gameState.IsUserOfType(userId, UserType.StoryTeller);
    }
}