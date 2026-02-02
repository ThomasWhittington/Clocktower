using Clocktower.Server.Common.Services;

namespace Clocktower.Server.Socket.Services;

public class GameBroadcastService(
    IGamePerspectiveStore gamePerspectiveStore,
    IDiscordTownManager discordTownManager,
    INotificationService notificationService) : IGameBroadcastService
{
    public async Task BroadcastDiscordTownUpdate(string gameId)
    {
        var perspectives = gamePerspectiveStore.GetAllPerspectivesForGame(gameId).ToList();
        if (!perspectives.Any()) return;
        var guildId = perspectives[0].GuildId;

        var notifications = BuildNotifications(gameId, guildId, perspectives);
        await notificationService.SendBulkDiscordTownUpdates(notifications);
    }

    public Task BroadcastTimeUpdate(string gameId, GameTime gameTime)
        => notificationService.SendTownTimeToGroup(gameId, gameTime);

    public Task BroadcastScriptUpdate(string gameId, Script script)
        => notificationService.SendScriptToGroup(gameId, script);

    public Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice, VoiceState voiceState)
        => notificationService.SendUserVoiceStateToGroup(gameId, userId, inVoice, voiceState);

    public Task BroadcastTimerUpdate(string gameId, TimerState timer)
        => notificationService.SendTimerUpdateToGroup(gameId, timer);

    public Task BroadcastNominationSessionUpdate(string gameId, NominationSession? nominationSession)
        => notificationService.SendNominationSessionUpdateToGroup(gameId, nominationSession);

    public Task PingUser(string userId, string message) => notificationService.PingUser(userId, message);

    private IEnumerable<UserNotification> BuildNotifications(string gameId, string guildId, List<GamePerspective> perspectives)
    {
        foreach (var perspective in perspectives.Where(p => p.UserId != IGamePerspectiveStore.OmniscientKey))
        {
            var town = CreateRedactedTownForPlayer(guildId, gameId, perspective);
            if (town != null)
            {
                yield return new UserNotification(perspective.UserId, town);
            }
        }

        var omniscientPerspective = perspectives.FirstOrDefault(p => p.UserId == IGamePerspectiveStore.OmniscientKey);
        if (omniscientPerspective != null)
        {
            var omniscientTown = discordTownManager.GetDiscordTownDto(guildId, gameId, omniscientPerspective.Users);
            if (omniscientTown != null)
            {
                var omniscientUsers = GetOmniscientUsers(omniscientPerspective);

                foreach (var user in omniscientUsers)
                {
                    yield return new UserNotification(user.Id, omniscientTown);
                }
            }
        }
    }

    private DiscordTownDto? CreateRedactedTownForPlayer(string guildId, string gameId, GamePerspective perspective)
    {
        var town = discordTownManager.GetDiscordTownDto(guildId, gameId, perspective.Users);
        return town == null ? null : discordTownManager.RedactTownDto(town, perspective.UserId);
    }

    private static IEnumerable<GameUser> GetOmniscientUsers(GamePerspective omniscientPerspective)
    {
        return omniscientPerspective.Users.Where(u => u.UserType is UserType.StoryTeller or UserType.Spectator);
    }
}