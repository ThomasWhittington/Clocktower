using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public class NotificationService(IHubContext<DiscordNotificationHub, IDiscordNotificationClient> hub, IGameStateStore gameStateStore, IDiscordTownStore discordTownStore)
    : INotificationService
{
    public Task BroadcastDiscordTownUpdate(string gameId)
    {
        var thisGame = gameStateStore.Get(gameId);
        if (thisGame is null)
            return Task.CompletedTask;
        var discordTown = discordTownStore.Get(thisGame.GuildId);
        if (discordTown is null)
            return Task.CompletedTask;
        var users = thisGame.Users;
        var enhancedTown = discordTown.ToDiscordTownDto(gameId, users);
        return hub.Clients.Group(GetGameGroupName(gameId)).DiscordTownUpdated(enhancedTown);
    }

    public Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice, VoiceState voiceState) => hub.Clients.Group(GetGameGroupName(gameId)).UserVoiceStateChanged(userId, inVoice, voiceState);
    public Task BroadcastTownTime(string gameId, GameTime gameTime) => hub.Clients.Group(GetGameGroupName(gameId)).TownTimeChanged((int)gameTime);
    public Task PingUser(string targetUserId, string message) => hub.Clients.User(targetUserId).PingUser(message);
    public Task BroadcastTimerUpdate(string gameId, TimerState timer) => hub.Clients.Group(GetGameGroupName(gameId)).TimerUpdated(timer);

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}