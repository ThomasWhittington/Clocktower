namespace Clocktower.Server.Data;

public record VoteHistoryRecord
{
    public DateTime Time { get; set; }
    public string? NominatorId { get; set; }
    public string? NomineeId { get; set; }
    public int VoteCount { get; set; }
    public int RequiredMajority { get; set; }
    public IEnumerable<string> Voters { get; set; } = [];
}