using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public interface IDiscordTownManager
{
    DiscordTown MoveUser(DiscordTown current, IDiscordGuildUser user, IDiscordVoiceChannel? newChannel);
    ChannelOccupants? FindUserChannel(DiscordTown occupants, string userId);
    bool UpdateUserStatus(ulong guildId, string userId, bool isPresent, VoiceState discordVoiceState);
    DiscordTown? GetDiscordTown(ulong guildId);
    TownUser? GetTownUser(string userId);
}