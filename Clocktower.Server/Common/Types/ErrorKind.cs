namespace Clocktower.Server.Common.Types;

public enum ErrorKind
{
    NotFound,
    Invalid,
    Forbidden,
    Conflict,
    External,
    Unexpected
}

public sealed record ErrorResponse(string Code, string Message);

public sealed record AppError(ErrorKind Kind, string Code, string Message);

public readonly record struct Result(AppError? Error)
{
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);

    public static Result<T> Fail<T>(AppError error) => Result<T>.Fail(error);

    public static Result<T> Fail<T>(ErrorKind kind, string code, string message) =>
        Result<T>.Fail(new AppError(kind, code, message));
}

public readonly record struct Result<T>(T? Value, AppError? Error)
{
    public bool IsSuccess => Error is null;
    public static Result<T> Ok(T value) => new(value, null);
    public static Result<T> Fail(AppError error) => new(default, error);
}

public static class Errors
{
    public static AppError GameNotFound(string gameId) =>
        new(ErrorKind.NotFound, "game.not_found", $"Couldn't find game with id: {gameId}");

    public static AppError InvalidGuildId() =>
        new(ErrorKind.Invalid, "guild.invalid_id", "GamePerspective contained a guildId that is not valid");

    public static AppError ChannelNotFound(string? channelId) =>
        new(ErrorKind.NotFound, "channel.not_found", $"The required channel could not be found: '{channelId}'");
}