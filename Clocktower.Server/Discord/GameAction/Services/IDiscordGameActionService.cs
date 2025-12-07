namespace Clocktower.Server.Discord.GameAction.Services;

public interface IDiscordGameActionService
{
    Task<(SetMuteAllPlayersOutcome outcome, string message)> SetMuteAllPlayersAsync(string gameId, bool muted);
}

public enum SetMuteAllPlayersOutcome
{
    GameDoesNotExistError,
    InvalidGuildError,
    PlayersUpdated
}