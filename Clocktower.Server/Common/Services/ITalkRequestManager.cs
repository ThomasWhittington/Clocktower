namespace Clocktower.Server.Common.Services;

public interface ITalkRequestManager
{
    IReadOnlyList<TalkRequest> GetTalkRequests(string gameId);
    Task AddTalkRequest(string gameId, string requesterId, string targetId);
    Task RemoveTalkRequest(string gameId, string requesterId, string targetId);
}