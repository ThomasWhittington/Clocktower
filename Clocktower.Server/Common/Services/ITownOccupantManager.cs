using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public interface ITownOccupantManager
{
    TownOccupants MoveUser(TownOccupants current, IDiscordGuildUser user, IDiscordVoiceChannel? newChannel);
    ChannelOccupants? FindUserChannel(TownOccupants occupants, string userId);
    bool UpdateUserStatus(ulong guildId, string userId, bool isPresent, VoiceState discordVoiceState);
    TownOccupants? GetTownOccupancy(ulong guildId);
    TownUser? GetTownUser(string userId);
}