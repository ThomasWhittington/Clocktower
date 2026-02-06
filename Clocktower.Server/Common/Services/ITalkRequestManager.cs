namespace Clocktower.Server.Common.Services;

public interface ITalkRequestManager
{
    List<TalkRequest> GetTalkRequests(string gameId);
    Task AddTalkRequest(string gameId, string requesterId, string targetId);
    Task RemoveTalkRequest(string gameId, string requesterId, string targetId);
}