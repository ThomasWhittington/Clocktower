namespace Clocktower.Server.Data.Types;

[UsedImplicitly]
public record JoinData(string GuildId, GameUser User, string GameId, string Jwt);