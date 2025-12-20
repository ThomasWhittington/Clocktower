using Clocktower.Server.Common.Services;

namespace Clocktower.Server.Socket.Services;

public class HubStateManager(IGamePerspectiveStore gamePerspectiveStore, IDiscordTownManager discordTownManager, IJwtWriter jwtWriter, ITimerCoordinator timerCoordinator) : IHubStateManager
{
    public SessionSyncState? GetState(string gameId, string userId)
    {
        var currentPerspective = gamePerspectiveStore.Get(gameId, userId);
        var gameUser = currentPerspective?.GetUser(userId);
        if (currentPerspective is null || gameUser is null) return null;
        var discordTown = discordTownManager.GetDiscordTown(currentPerspective.GuildId);
        var enhancedTown = discordTown?.ToDiscordTownDto(currentPerspective.Id, currentPerspective.Users);

        if (enhancedTown != null && gameUser.UserType == UserType.Player)
            enhancedTown = discordTownManager.RedactTownDto(enhancedTown, userId);

        var timer = timerCoordinator.Get(gameId);

        var currentState = new SessionSyncState
        {
            GameTime = currentPerspective.GameTime,
            Jwt = jwtWriter.GetJwtToken(gameUser),
            DiscordTown = enhancedTown,
            Timer = timer
        };
        return currentState;
    }
}

public interface IHubStateManager
{
    SessionSyncState? GetState(string gameId, string userId);
}