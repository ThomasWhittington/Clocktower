using Clocktower.Server.Common.Services;
using Clocktower.Server.Socket.Services;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public sealed class NotificationHub(IHubStateManager hubStateManager, IVotingService votingService) : Hub<INotificationClient>, INotificationHub
{
    [UsedImplicitly]
    public async Task<SessionSyncState?> JoinGameGroup(string gameId, string userId, string? oldGameId = null)
    {
        if (!string.IsNullOrEmpty(oldGameId) && oldGameId != gameId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGameGroupName(oldGameId));
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGameGroupName(gameId));

        var currentState = hubStateManager.GetState(gameId, userId);
        return currentState;
    }

    [UsedImplicitly]
    public Task LeaveGameGroup(string gameId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGameGroupName(gameId));

    public async Task OpenNominations(string gameId)
        => await votingService.OpenNominations(gameId);

    public async Task CloseNominations(string gameId)
        => await votingService.CloseNominations(gameId);

    public async Task StartVote(string gameId, int votingSpeed)
        => await votingService.StartVote(gameId, votingSpeed);

    public async Task<bool> MakeNomination(string gameId, string nominatorId, string nomineeId)
        => await votingService.MakeNomination(gameId, nominatorId, nomineeId);


    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}