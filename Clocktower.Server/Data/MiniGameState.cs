namespace Clocktower.Server.Data;

[UsedImplicitly]
public record MiniGameState(string GameId, GameUser CreatedBy, DateTime CreatedDate);