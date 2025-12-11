using Clocktower.Server.Data.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public class NotificationService(IHubContext<DiscordNotificationHub, IDiscordNotificationClient> hub, IGameStateStore gameStateStore)
    : INotificationService
{
    public Task BroadcastDiscordTownUpdate(string gameId, DiscordTown discordTown)
    {
        var thisGame = gameStateStore.Get(gameId);
        var users = thisGame?.Users;
        var enhancedTown = discordTown.ToDiscordTownDto(gameId, users);
        return hub.Clients.Group(GetGameGroupName(gameId)).DiscordTownUpdated(enhancedTown);
    }

    public Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice, VoiceState voiceState) => hub.Clients.Group(GetGameGroupName(gameId)).UserVoiceStateChanged(userId, inVoice, voiceState);
    public Task BroadcastTownTime(string gameId, GameTime gameTime) => hub.Clients.Group(GetGameGroupName(gameId)).TownTimeChanged((int)gameTime);
    public Task PingUser(string targetUserId, string message) => hub.Clients.User(targetUserId).PingUser(message);

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}