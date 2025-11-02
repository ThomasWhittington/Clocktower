using Clocktower.Server.Discord.Services;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public interface IDiscordNotificationClient
{
    Task TownOccupancyUpdated(TownOccupants townOccupants);
}

public sealed class DiscordNotificationHub : Hub<IDiscordNotificationClient>;