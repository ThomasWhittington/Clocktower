namespace Clocktower.Server.Common.Services;

public interface IVotingService : IHostedService
{
    Task OpenNominations(string gameId);
    Task CloseNominations(string gameId);
    Task StartVote(string gameId, int votingSpeed);
    Task<bool> MakeNomination(string gameId, string nominatorId, string nomineeId);
    NominationSession? GetSession(string gameId);
}