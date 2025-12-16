using Clocktower.Server.Common.Services;

namespace Clocktower.Server.Socket.Services;

public class HubStateManager(IGameStateStore gameStateStore, IDiscordTownManager discordTownManager, IJwtWriter jwtWriter, ITimerCoordinator timerCoordinator) : IHubStateManager
{
    public SessionSyncState? GetState(string gameId, string userId)
    {
        var currentGameState = gameStateStore.Get(gameId);
        var gameUser = currentGameState?.GetUser(userId);
        if (currentGameState is null || gameUser is null) return null;
        var discordTown = discordTownManager.GetDiscordTown(currentGameState.GuildId);
        var enhancedTown = discordTown?.ToDiscordTownDto(currentGameState.Id, currentGameState.Users);

        if (enhancedTown != null && gameUser.UserType == UserType.Player)
            enhancedTown = discordTownManager.RedactTownDto(enhancedTown, userId);

        var timer = timerCoordinator.Get(gameId);

        var currentState = new SessionSyncState
        {
            GameTime = currentGameState.GameTime,
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