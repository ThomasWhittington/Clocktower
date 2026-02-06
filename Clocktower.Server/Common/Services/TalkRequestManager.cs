using Clocktower.Server.Socket.Services;

namespace Clocktower.Server.Common.Services;

public class TalkRequestManager(IGameBroadcastService gameBroadcastService) : ITalkRequestManager
{
    private readonly Dictionary<string, List<TalkRequest>> _talkRequests = new();

    public async Task AddTalkRequest(string gameId, string requesterId, string targetId)
    {
        if (!_talkRequests.ContainsKey(gameId))
            _talkRequests[gameId] = [];

        var existing = _talkRequests[gameId]
            .FirstOrDefault(r => r.RequesterId == requesterId && r.TargetId == targetId);

        if (existing != null) return;
        _talkRequests[gameId].Add(new TalkRequest
        {
            RequesterId = requesterId,
            TargetId = targetId,
            Timestamp = DateTime.UtcNow
        });

        await gameBroadcastService.BroadcastTalkRequestsUpdate(gameId, _talkRequests[gameId]);
    }

    public List<TalkRequest> GetTalkRequests(string gameId)
    {
        return _talkRequests.GetValueOrDefault(gameId) ?? [];
    }

    public async Task RemoveTalkRequest(string gameId, string requesterId, string targetId)
    {
        if (!_talkRequests.TryGetValue(gameId, out var request)) return;
        request.RemoveAll(r =>
            (r.RequesterId == requesterId && r.TargetId == targetId) ||
            (r.RequesterId == targetId && r.TargetId == requesterId));

        await gameBroadcastService.BroadcastTalkRequestsUpdate(gameId, _talkRequests[gameId]);
    }
}