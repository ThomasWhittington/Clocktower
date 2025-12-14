namespace Clocktower.Server.Discord.GameAction.Services;

public interface IDiscordGameActionService
{
    Task<Result<string>> SetMuteAllPlayersAsync(string gameId, bool muted);
    Task<Result<string>> SendToCottagesAsync(string gameId);
    Task<Result<string>> SendToTownSquareAsync(string gameId);
}

public enum GameActionOutcome
{
    GameDoesNotExistError,
    InvalidGuildError,
    ActionCompleted,
    ChannelNotFound,
    NotEnoughChannels
}