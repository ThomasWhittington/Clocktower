using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public interface ITownOccupantManager
{
    TownOccupants MoveUser(TownOccupants current, IDiscordUser user, IDiscordVoiceChannel? newChannel);
}