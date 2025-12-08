using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public interface ITownOccupantManager
{
    TownOccupants MoveUser(TownOccupants current, IDiscordGuildUser user, IDiscordVoiceChannel? newChannel);
    ChannelOccupants? FindUserChannel(TownOccupants occupants, string userId);
    TownOccupants? UpdateUserStatus(ulong guildId, ulong userId, bool isPresent, VoiceState discordVoiceState);
}