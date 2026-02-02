using System.Collections.Concurrent;
using Clocktower.Server.Socket.Services;

namespace Clocktower.Server.Common.Services;

//TODO check that cases are valid before changing things, no nominations if nominations not enabled etc
public class VotingService(IGamePerspectiveService gamePerspectiveService, IGameBroadcastService gameBroadcastService) : BackgroundService, IVotingService
{
    private readonly ConcurrentDictionary<string, NominationSession> _sessions = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(100, stoppingToken);

            foreach (var session in _sessions.Values.Where(s => s.VoteUnderway))
            {
                await ProcessSession(session);
            }
        }
    }

    private async Task ProcessSession(NominationSession session)
    {
        if (DateTime.UtcNow < session.NextTick) return;

        if (session.CountDown > 0)
        {
            session.CountDown--;
            session.NextTick = DateTime.UtcNow.AddSeconds(1);
            await gameBroadcastService.BroadcastNominationSessionUpdate(session.GameId, session);
            return;
        }

        if (session.CountDown != 0) await LockVote(session);

        session.CountDown = null;
        session.CurrentTarget++;
        if (session.CurrentTarget >= session.PlayerCount) session.CurrentTarget = 0;

        session.NextTick = DateTime.UtcNow.AddMilliseconds(session.VotingSpeed);

        await gameBroadcastService.BroadcastNominationSessionUpdate(session.GameId, session);

        if (session.CurrentTarget != session.Nominee) return;
        await Task.Delay(session.VotingSpeed + 100);

        await LockVote(session);
        session.VoteUnderway = false;
        await gameBroadcastService.BroadcastNominationSessionUpdate(session.GameId, session);
        await gameBroadcastService.BroadcastDiscordTownUpdate(session.GameId);
    }

    public async Task<bool> ToggleVote(string gameId, string playerId)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return false;
        var user = gamePerspective.Users.FirstOrDefault(o => o.Id == playerId);
        if (user is null) return false;

        var updated = gamePerspectiveService.UpdatePublicUser(gameId, playerId, new PublicGameUserUpdate
        {
            HandUp = !user.HandUp
        });
        if (updated) await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
        return updated;
    }

    public async Task<bool> MakeNomination(string gameId, string nominatorId, string nomineeId)
    {
        var session = GetSession(gameId);
        if (session is null) return false;

        var game = gamePerspectiveService.GetFirstPerspective(gameId);
        if (game is null) return false;
        var nominator = game.Players.FirstOrDefault(o => o.Id == nominatorId);
        var nominee = game.Players.FirstOrDefault(o => o.Id == nomineeId);

        session = session with
        {
            Nominator = nominator?.SeatingPosition,
            Nominee = nominee?.SeatingPosition,
            CurrentTarget = nominee?.SeatingPosition
        };

        _sessions[gameId] = session;

        await gameBroadcastService.BroadcastNominationSessionUpdate(gameId, session);
        return true;
    }

    public async Task OpenNominations(string gameId)
    {
        var game = gamePerspectiveService.GetFirstPerspective(gameId);
        if (game is null) return;

        var session = new NominationSession(gameId)
        {
            VoteUnderway = false,
            PlayerCount = game.Players.Count()
        };

        _sessions[gameId] = session;

        await gameBroadcastService.BroadcastNominationSessionUpdate(gameId, session);
    }

    public async Task CloseNominations(string gameId)
    {
        var session = GetSession(gameId);
        if (session is null) return;

        _sessions.Remove(gameId, out _);

        gamePerspectiveService.ResetNominationSession(gameId);

        await gameBroadcastService.BroadcastNominationSessionUpdate(gameId, null);
        await gameBroadcastService.BroadcastDiscordTownUpdate(session.GameId);
    }

    public async Task StartVote(string gameId, int votingSpeed)
    {
        var session = GetSession(gameId);
        if (session?.Nominator == null || session.Nominee == null) return;
        var newSession = session with
        {
            VoteUnderway = true,
            VotingSpeed = votingSpeed,
            NextTick = DateTime.UtcNow.AddSeconds(1),
            CountDown = 3
        };

        _sessions[gameId] = newSession;

        await gameBroadcastService.BroadcastNominationSessionUpdate(gameId, newSession);
    }

    public NominationSession? GetSession(string gameId)
    {
        return _sessions.TryGetValue(gameId, out var session) ? session : null;
    }

    private async Task LockVote(NominationSession session)
    {
        var game = gamePerspectiveService.GetFirstPerspective(session.GameId);
        if (game is null) return;
        var user = game.Players.FirstOrDefault(o => o.SeatingPosition == session.CurrentTarget);
        if (user is null) return;
        gamePerspectiveService.UpdatePublicUser(session.GameId, user.Id, new PublicGameUserUpdate { VoteLocked = true });
        await gameBroadcastService.BroadcastDiscordTownUpdate(session.GameId);
    }
}