namespace Clocktower.Server.Common.Api.Auth;

public interface IGameAuthorizationService
{
    bool IsStoryTellerForGame(string userId, string gameId);
}