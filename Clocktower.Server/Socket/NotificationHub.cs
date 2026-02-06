using Clocktower.Server.Common.Services;
using Clocktower.Server.Socket.Services;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Socket;

public sealed class NotificationHub(IHubStateManager hubStateManager, IGamePerspectiveService gamePerspectiveService, IVotingService votingService, ITalkRequestManager talkRequestManager) : Hub<INotificationClient>, INotificationHub
{
    [UsedImplicitly]
    public async Task<SessionSyncState?> JoinGameGroup(string gameId, string userId, string? oldGameId = null)
    {
        if (!string.IsNullOrEmpty(oldGameId) && oldGameId != gameId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGameGroupName(oldGameId));
        }

        if (!gamePerspectiveService.GameExists(gameId)) return null;

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

    public async Task CancelVote(string gameId)
        => await votingService.CancelVote(gameId);

    public async Task NextNomination(string gameId)
        => await votingService.NextNomination(gameId);

    public async Task RemoveAllMarks(string gameId)
        => await votingService.RemoveAllMarks(gameId);

    public async Task<bool> MakeNomination(string gameId, string nominatorId, string nomineeId)
        => await votingService.MakeNomination(gameId, nominatorId, nomineeId);

    public async Task<bool> ToggleMarkPlayer(string gameId, string playerId)
        => await votingService.ToggleMarkPlayer(gameId, playerId);

    public IEnumerable<VoteHistoryRecord> GetVoteHistory(string gameId)
        => votingService.GetVoteHistory(gameId);

    public async Task<bool> ToggleVote(string gameId, string playerId)
        => await votingService.ToggleVote(gameId, playerId);

    public async Task RequestToTalk(string gameId, string requesterId, string targetId)
        => await talkRequestManager.AddTalkRequest(gameId, requesterId, targetId);

    private static string GetGameGroupName(string gameId) => $"game:{gameId}";
}