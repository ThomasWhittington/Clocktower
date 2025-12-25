namespace Clocktower.Server.Common.Api.Auth;

public class GameAuthorizationService(IGamePerspectiveStore gamePerspectiveStore) : IGameAuthorizationService
{
    public bool IsStoryTellerForGame(string userId, string gameId)
    {
        var gamePerspective = gamePerspectiveStore.Get(gameId, userId);
        return gamePerspective is not null && gamePerspective.IsUserOfType(userId, UserType.StoryTeller);
    }
}