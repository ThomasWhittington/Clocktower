namespace Clocktower.Server.Socket;

public interface INotificationHub
{
    Task<SessionSyncState?> JoinGameGroup(string gameId, string userId, string? oldGameId = null);
    Task LeaveGameGroup(string gameId);

    Task OpenNominations(string gameId);
    Task CloseNominations(string gameId);
    Task StartVote(string gameId, int votingSpeed);
    Task CancelVote(string gameId);
    Task NextNomination(string gameId);
    Task RemoveAllMarks(string gameId);
    Task<bool> ToggleVote(string gameId, string playerId);
    Task<bool> MakeNomination(string gameId, string nominatorId, string nomineeId);
    Task<bool> ToggleMarkPlayer(string gameId, string playerId);
}