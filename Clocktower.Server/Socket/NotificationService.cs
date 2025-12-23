using Clocktower.Server.Common.Services;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public class NotificationService(IHubContext<DiscordNotificationHub, IDiscordNotificationClient> hub, IGamePerspectiveStore gamePerspectiveStore, IDiscordTownManager discordTownManager)
    : INotificationService
{
    public async Task BroadcastDiscordTownUpdate(string gameId)
    {
        var perspectives = gamePerspectiveStore.GetAllPerspectivesForGame(gameId).ToList();
        if (!perspectives.Any()) return;
        var guildId = perspectives[0].GuildId;

        var tasks = new List<Task>();

        foreach (var perspective in perspectives)
        {
            var currentUser = perspective.Users.FirstOrDefault(u => u.Id == perspective.UserId);
            bool needsRedaction = currentUser?.UserType is UserType.Player;

            var thisTown = discordTownManager.GetDiscordTownDto(guildId, gameId, perspective.Users);
            if (thisTown is null) continue;
            if (needsRedaction) thisTown = discordTownManager.RedactTownDto(thisTown, perspective.UserId);

            var task = hub.Clients.User(perspective.UserId).DiscordTownUpdated(thisTown);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    public Task BroadcastUserVoiceStateChanged(string gameId, string userId, bool inVoice, VoiceState voiceState) => hub.Clients.Group(GetGameGroupName(gameId)).UserVoiceStateChanged(userId, inVoice, voiceState);
    public Task BroadcastTownTime(string gameId, GameTime gameTime) => hub.Clients.Group(GetGameGroupName(gameId)).TownTimeChanged((int)gameTime);
    public Task PingUser(string targetUserId, string message) => hub.Clients.User(targetUserId).PingUser(message);
    public Task BroadcastTimerUpdate(string gameId, TimerState timer) => hub.Clients.Group(GetGameGroupName(gameId)).TimerUpdated(timer);

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}