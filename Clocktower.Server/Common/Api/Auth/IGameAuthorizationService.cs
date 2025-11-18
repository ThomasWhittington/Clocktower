namespace Clocktower.Server.Common.Api.Auth;

public interface IGameAuthorizationService
{
    bool IsStoryTellerForGameAsync(string userId, string gameId);
}