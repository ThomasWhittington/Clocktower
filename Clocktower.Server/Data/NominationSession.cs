namespace Clocktower.Server.Data;

public record NominationSession(string GameId)
{
    public bool VoteUnderway { get; set; }
    public int? Nominee { get; set; }
    public int? Nominator { get; set; }
    public int? CurrentTarget { get; set; }
    public int VotingSpeed { get; set; }
    public int PlayerCount { get; set; }
    public int? CountDown { get; set; }
    public DateTime NextTick { get; set; }
}