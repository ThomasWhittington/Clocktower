using System.Collections.Concurrent;
using Clocktower.Server.Socket;

namespace Clocktower.Server.Common.Services;

public class VotingService(IGamePerspectiveService gamePerspectiveService, INotificationService notificationService) : BackgroundService, IVotingService
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
            await notificationService.SendNominationUpdateToGroup(session.GameId, session);
            return;
        }

        session.CountDown = null;
        session.CurrentTarget++;
        if (session.CurrentTarget >= session.PlayerCount) session.CurrentTarget = 0;

        session.NextTick = DateTime.UtcNow.AddMilliseconds(session.VotingSpeed);

        await notificationService.SendNominationUpdateToGroup(session.GameId, session);

        if (session.CurrentTarget != session.Nominee) return;
        await Task.Delay(session.VotingSpeed + 100);

        session.VoteUnderway = false;
        await notificationService.SendNominationUpdateToGroup(session.GameId, session);
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

        await notificationService.SendNominationUpdateToGroup(gameId, session);
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

        await notificationService.SendNominationUpdateToGroup(gameId, session);
    }

    public async Task CloseNominations(string gameId)
    {
        var session = GetSession(gameId);
        if (session is null) return;

        _sessions.Remove(gameId, out _);
        await notificationService.SendNominationUpdateToGroup(gameId, null);
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

        await notificationService.SendNominationUpdateToGroup(gameId, newSession);
    }

    public NominationSession? GetSession(string gameId)
    {
        return _sessions.TryGetValue(gameId, out var session) ? session : null;
    }
}