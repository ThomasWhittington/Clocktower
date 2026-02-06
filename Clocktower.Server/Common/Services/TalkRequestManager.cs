using Clocktower.Server.Socket.Services;

namespace Clocktower.Server.Common.Services;

public class TalkRequestManager(IGameBroadcastService gameBroadcastService) : ITalkRequestManager
{
    private readonly Dictionary<string, List<TalkRequest>> _talkRequests = new();
    private readonly Lock _lock = new();

    public async Task AddTalkRequest(string gameId, string requesterId, string targetId)
    {
        List<TalkRequest> snapshot;
        lock (_lock)
        {
            if (!_talkRequests.ContainsKey(gameId))
                _talkRequests[gameId] = [];

            var list = _talkRequests[gameId];
            if (list.Any(r => r.RequesterId == requesterId && r.TargetId == targetId))
                return;

            list.Add(new TalkRequest
            {
                RequesterId = requesterId,
                TargetId = targetId,
                Timestamp = DateTime.UtcNow
            });
            snapshot = [.. list];
        }

        await gameBroadcastService.BroadcastTalkRequestsUpdate(gameId, snapshot);
    }

    public IReadOnlyList<TalkRequest> GetTalkRequests(string gameId)
    {
        lock (_lock)
        {
            return _talkRequests.TryGetValue(gameId, out var list) ? [.. list] : [];
        }
    }

    public async Task RemoveTalkRequest(string gameId, string requesterId, string targetId)
    {
        List<TalkRequest> snapshot;
        lock (_lock)
        {
            if (!_talkRequests.TryGetValue(gameId, out var list)) return;
            list.RemoveAll(r =>
                (r.RequesterId == requesterId && r.TargetId == targetId) ||
                (r.RequesterId == targetId && r.TargetId == requesterId));
            snapshot = [.. list];
        }

        await gameBroadcastService.BroadcastTalkRequestsUpdate(gameId, snapshot);
    }
}