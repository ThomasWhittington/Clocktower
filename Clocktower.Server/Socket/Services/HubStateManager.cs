using Clocktower.Server.Common.Services;

namespace Clocktower.Server.Socket.Services;

public class HubStateManager(IGamePerspectiveStore gamePerspectiveStore, IDiscordTownManager discordTownManager, IJwtWriter jwtWriter, ITimerCoordinator timerCoordinator) : IHubStateManager
{
    public SessionSyncState? GetState(string gameId, string userId)
    {
        var currentPerspective = gamePerspectiveStore.Get(gameId, userId);
        var gameUser = currentPerspective?.GetUser(userId);
        if (currentPerspective is null || gameUser is null) return null;
        var discordTown = discordTownManager.GetDiscordTownDto(currentPerspective.GuildId, currentPerspective.Id, currentPerspective.Users);

        if (discordTown != null && gameUser.UserType == UserType.Player)
            discordTown = discordTownManager.RedactTownDto(discordTown, userId);

        var timer = timerCoordinator.Get(gameId);

        var currentState = new SessionSyncState
        {
            GameTime = currentPerspective.GameTime,
            Jwt = jwtWriter.GetJwtToken(gameUser),
            DiscordTown = discordTown,
            Timer = timer
        };
        return currentState;
    }
}

public interface IHubStateManager
{
    SessionSyncState? GetState(string gameId, string userId);
}