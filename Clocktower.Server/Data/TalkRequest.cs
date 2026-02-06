namespace Clocktower.Server.Data;

public class TalkRequest
{
    public string RequesterId { get; set; }
    public string TargetId { get; set; }
    public DateTime Timestamp { get; set; }
}