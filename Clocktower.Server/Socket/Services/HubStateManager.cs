using Clocktower.Server.Common.Services;

namespace Clocktower.Server.Socket.Services;

public class HubStateManager(
    IGamePerspectiveService gamePerspectiveService,
    IDiscordTownManager discordTownManager,
    IJwtWriter jwtWriter,
    ITimerCoordinator timerCoordinator,
    IVotingService votingService
) : IHubStateManager
{
    public SessionSyncState? GetState(string gameId, string userId)
    {
        var currentPerspective = gamePerspectiveService.GetPerspective(gameId, userId);
        var gameUser = currentPerspective?.GetUser(userId);
        if (currentPerspective is null || gameUser is null) return null;
        var discordTown = discordTownManager.GetDiscordTownDto(currentPerspective.GuildId, currentPerspective.Id, currentPerspective.Users);

        if (discordTown != null && gameUser.UserType == UserType.Player)
            discordTown = discordTownManager.RedactTownDto(discordTown, userId);

        var timer = timerCoordinator.Get(gameId);
        var nominationSession = votingService.GetSession(gameId);

        var jwtToken = jwtWriter.GetJwtToken(gameUser);

        var currentState = new SessionSyncState
        {
            GameTime = currentPerspective.GameTime,
            Script = currentPerspective.Script,
            Jwt = jwtToken,
            DiscordTown = discordTown,
            Timer = timer,
            NominationSession = nominationSession
        };
        return currentState;
    }
}