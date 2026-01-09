using Clocktower.Server.Common.Services;

namespace Clocktower.Server.Common.Api.Auth;

public class GameAuthorizationService(IGamePerspectiveService gamePerspectiveService) : IGameAuthorizationService
{
    public bool IsStoryTellerForGame(string userId, string gameId)
    {
        var gamePerspective = gamePerspectiveService.GetPerspective(gameId, userId);
        return gamePerspective is not null && gamePerspective.IsUserOfType(userId, UserType.StoryTeller);
    }
}