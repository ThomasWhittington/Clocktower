using System.Collections.Concurrent;
using Clocktower.Server.Socket.Services;

namespace Clocktower.Server.Common.Services;

public class VotingService(IGamePerspectiveService gamePerspectiveService, IGameBroadcastService gameBroadcastService) : BackgroundService, IVotingService
{
    private readonly ConcurrentDictionary<string, NominationSession> _sessions = new();
    private readonly ConcurrentDictionary<string, VoteHistoryRecord[]> _voteHistory = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(100, stoppingToken);

            foreach (var session in _sessions.Values.Where(s => s.VoteUnderway))
            {
                await ProcessSession(session, stoppingToken);
            }
        }
    }

    private async Task ProcessSession(NominationSession session, CancellationToken stoppingToken)
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
        await Task.Delay(session.VotingSpeed + 100, stoppingToken);

        await LockVote(session);
        await EndVote(session);
    }

    public async Task CancelVote(string gameId)
    {
        var session = GetSession(gameId);
        if (session is null || !session.VoteUnderway) return;

        var game = gamePerspectiveService.GetFirstPerspective(gameId);
        if (game is not null)
        {
            foreach (var player in game.Players)
            {
                gamePerspectiveService.UpdatePublicUser(gameId, player.Id, new PublicGameUserUpdate
                {
                    VoteLocked = false
                });
            }
        }

        var updatedSession = session with
        {
            VoteUnderway = false,
            VoteEnded = false,
            CurrentTarget = session.Nominee,
            CountDown = null,
            NextTick = default
        };

        _sessions[gameId] = updatedSession;

        await gameBroadcastService.BroadcastNominationSessionUpdate(gameId, updatedSession);
        await gameBroadcastService.BroadcastPlayAudio(gameId, AudioEvent.Stop);
        await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
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
        await gameBroadcastService.BroadcastPlayAudio(gameId, AudioEvent.Nomination);
        return true;
    }

    public async Task OpenNominations(string gameId)
    {
        var game = gamePerspectiveService.GetFirstPerspective(gameId);
        if (game is null) return;

        var session = new NominationSession(gameId)
        {
            VoteUnderway = false,
            VoteEnded = false,
            PlayerCount = game.Players.Count(),
            RequiredMajority = GetRequiredMajority(game.Players)
        };

        _sessions[gameId] = session;

        await gameBroadcastService.BroadcastNominationSessionUpdate(gameId, session);
        await gameBroadcastService.BroadcastPlayAudio(gameId, AudioEvent.NominationsOpen);
    }

    private static int GetRequiredMajority(IEnumerable<GameUser> players)
    {
        var applicablePlayers = players.Where(o => !o.IsDead);
        return (int)Math.Ceiling(applicablePlayers.Count() / 2.0);
    }

    public async Task<bool> ToggleMarkPlayer(string gameId, string playerId)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return false;
        var user = gamePerspective.Users.FirstOrDefault(o => o.Id == playerId);
        if (user is null) return false;

        var userNowMarked = !user.IsMarked;
        var updated = gamePerspectiveService.UpdatePublicUser(gameId, playerId, new PublicGameUserUpdate
        {
            IsMarked = userNowMarked
        });

        if (!updated) return updated;

        if (userNowMarked)
        {
            var otherUsers = gamePerspective.Users.Where(o => o.Id != playerId && o.IsMarked).ToList();
            foreach (var otherUser in otherUsers)
            {
                gamePerspectiveService.UpdatePublicUser(gameId, otherUser.Id, new PublicGameUserUpdate
                {
                    IsMarked = false
                });
            }

            await gameBroadcastService.BroadcastPlayAudio(gameId, AudioEvent.PlayerMarked);
        }

        await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
        return updated;
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

    public async Task NextNomination(string gameId)
    {
        var session = GetSession(gameId);
        if (session is null) return;

        var newSession = session with
        {
            VoteUnderway = false,
            VoteEnded = false,
            Nominator = null,
            Nominee = null,
            CurrentTarget = null
        };

        _sessions[gameId] = newSession;

        gamePerspectiveService.ResetNominationSession(gameId);

        await gameBroadcastService.BroadcastNominationSessionUpdate(gameId, newSession);
        await gameBroadcastService.BroadcastDiscordTownUpdate(session.GameId);
    }

    public async Task StartVote(string gameId, int votingSpeed)
    {
        if (votingSpeed <= 0) return;
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
        await gameBroadcastService.BroadcastPlayAudio(session.GameId, AudioEvent.Countdown);
    }

    public NominationSession? GetSession(string gameId)
    {
        return _sessions.TryGetValue(gameId, out var session) ? session : null;
    }

    public async Task RemoveAllMarks(string gameId)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return;
        var users = gamePerspective.Users.Where(o => o.IsMarked).ToList();
        if (!users.Any()) return;

        bool updated = false;
        foreach (var user in users)
        {
            var thisUpdate = gamePerspectiveService.UpdatePublicUser(gameId, user.Id, new PublicGameUserUpdate
            {
                IsMarked = !user.IsMarked
            });
            if (thisUpdate) updated = true;
        }

        if (updated) await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
    }

    public IEnumerable<VoteHistoryRecord> GetVoteHistory(string gameId)
    {
        return _voteHistory.TryGetValue(gameId, out var voteHistory) ? voteHistory : [];
    }

    private async Task LockVote(NominationSession session)
    {
        var game = gamePerspectiveService.GetFirstPerspective(session.GameId);
        if (game is null) return;
        var user = game.Players.FirstOrDefault(o => o.SeatingPosition == session.CurrentTarget);
        if (user is null) return;
        gamePerspectiveService.UpdatePublicUser(session.GameId, user.Id, new PublicGameUserUpdate { VoteLocked = true });
        await gameBroadcastService.BroadcastDiscordTownUpdate(session.GameId);
        await gameBroadcastService.BroadcastPlayAudio(session.GameId, user.HandUp ? AudioEvent.HandPassUp : AudioEvent.HandPassDown);
    }

    private async Task EndVote(NominationSession session)
    {
        session.VoteUnderway = false;
        session.VoteEnded = true;
        await gameBroadcastService.BroadcastNominationSessionUpdate(session.GameId, session);
        await gameBroadcastService.BroadcastDiscordTownUpdate(session.GameId);

        var game = gamePerspectiveService.GetFirstPerspective(session.GameId);
        if (game is null) return;

        var nominator = game.Players.FirstOrDefault(o => o.SeatingPosition == session.Nominator);
        var nominee = game.Players.FirstOrDefault(o => o.SeatingPosition == session.Nominee);
        var voters = game.Players.Where(o => o.HandUp).Select(o => o.Id).ToArray();

        var voteHistory = new VoteHistoryRecord
        {
            Time = DateTime.UtcNow,
            NominatorId = nominator?.Id ?? string.Empty,
            NomineeId = nominee?.Id ?? string.Empty,
            VoteCount = voters.Length,
            RequiredMajority = session.RequiredMajority,
            Voters = voters
        };

        _voteHistory.AddOrUpdate(
            session.GameId,
            [voteHistory],
            (_, existingHistory) => [..existingHistory, voteHistory]
        );
    }
}