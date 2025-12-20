using Clocktower.Server.Common.Services;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public class NotificationService(IHubContext<DiscordNotificationHub, IDiscordNotificationClient> hub, IGamePerspectiveStore gamePerspectiveStore, IDiscordTownManager discordTownManager)
    : INotificationService
{
    //TODO will need a rework to ensure the correct game perspective is retrieved per player
    public async Task BroadcastDiscordTownUpdate(string gameId)
    {
        var thisGame = gamePerspectiveStore.GetFirstPerspective(gameId);
        if (thisGame is null) return;
        var discordTown = discordTownManager.GetDiscordTown(thisGame.GuildId);
        if (discordTown is null) return;
        var users = thisGame.Users;

        var fullAccessUserIds = users.Where(u => u.UserType is UserType.StoryTeller or UserType.Spectator).GetIds().ToArray();
        var playerUserIds = users.Where(u => u.UserType is UserType.Player).GetIds().ToArray();

        var fullTown = discordTown.ToDiscordTownDto(gameId, users);
        await hub.Clients.Users(fullAccessUserIds).DiscordTownUpdated(fullTown);
        var playerTasks = playerUserIds.Select(playerId =>
        {
            var perPlayerTown = discordTownManager.RedactTownDto(fullTown, playerId);
            return hub.Clients.User(playerId).DiscordTownUpdated(perPlayerTown);
        });
        await Task.WhenAll(playerTasks);
    }

    public Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice, VoiceState voiceState) => hub.Clients.Group(GetGameGroupName(gameId)).UserVoiceStateChanged(userId, inVoice, voiceState);
    public Task BroadcastTownTime(string gameId, GameTime gameTime) => hub.Clients.Group(GetGameGroupName(gameId)).TownTimeChanged((int)gameTime);
    public Task PingUser(string targetUserId, string message) => hub.Clients.User(targetUserId).PingUser(message);
    public Task BroadcastTimerUpdate(string gameId, TimerState timer) => hub.Clients.Group(GetGameGroupName(gameId)).TimerUpdated(timer);

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}