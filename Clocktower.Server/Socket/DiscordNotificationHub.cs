using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public interface IDiscordNotificationClient
{
    Task TownOccupancyUpdated(TownOccupants townOccupants);
    Task UserVoiceStateChanged(string userId, bool isInVoice);
    Task TownTimeChanged(int gameTime);
    Task PingUser(string message);
}

public sealed class DiscordNotificationHub : Hub<IDiscordNotificationClient>
{
    [UsedImplicitly]
    public Task JoinGameGroup(string gameId) => Groups.AddToGroupAsync(Context.ConnectionId, GetGameGroupName(gameId));

    [UsedImplicitly]
    public Task LeaveGameGroup(string gameId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGameGroupName(gameId));

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}