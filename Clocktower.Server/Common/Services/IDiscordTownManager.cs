using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public interface IDiscordTownManager
{
    DiscordTown MoveUser(DiscordTown current, IDiscordGuildUser user, IDiscordVoiceChannel? newChannel);
    ChannelOccupants? FindUserChannel(DiscordTown occupants, string userId);
    bool UpdateUserStatus(string guildId, string userId, bool isPresent, VoiceState discordVoiceState);
    DiscordTown? GetDiscordTown(string guildId);
    string? GetVoiceChannelIdByName(string guildId, string voiceChannelName);
    TownUser? GetTownUser(string userId);
    IEnumerable<MiniChannel> GetNightChannels(string guildId, string categoryName);
    DiscordTownDto RedactTownDto(DiscordTownDto discordTownDto, string userId);
}