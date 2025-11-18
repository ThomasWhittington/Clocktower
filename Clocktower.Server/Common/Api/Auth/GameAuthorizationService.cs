using Clocktower.Server.Data.Extensions;

namespace Clocktower.Server.Common.Api.Auth;

public class GameAuthorizationService : IGameAuthorizationService
{
    public bool IsStoryTellerForGameAsync(string userId, string gameId)
    {
        var gameState = GameStateStore.Get(gameId);
        return gameState is not null && gameState.IsUserOfType(userId, UserType.StoryTeller);
    }
}