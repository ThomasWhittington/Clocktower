namespace Clocktower.Server.Data.Types;

public enum InviteUserOutcome
{
    InviteSent,
    GameDoesNotExistError,
    InvalidGuildError,
    UserNotFoundError,
    DmChannelFailed,
    UnknownError
}