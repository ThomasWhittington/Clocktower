using Clocktower.Server.Discord.Services;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public class StateHub : Hub
{
    public async Task UserMovedChannel(TownOccupants townOccupants)
    {
        await Clients.All.SendAsync("UserMovedChannel", townOccupants);
    }
}