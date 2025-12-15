using Discord;

namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordGuild
{
    string Id { get; }
    string Name { get; }
    IEnumerable<IDiscordGuildUser> Users { get; }
    IEnumerable<IDiscordRole> Roles { get; }
    IEnumerable<IDiscordVoiceChannel> VoiceChannels { get; }
    IEnumerable<IDiscordCategoryChannel> CategoryChannels { get; }
    IDiscordRole EveryoneRole { get; }
    Task<IDiscordRestCategoryChannel> CreateCategoryChannelAsync(string categoryName, Action<GuildChannelProperties> func);
    Task<IDiscordRestVoiceChannel> CreateVoiceChannelAsync(string channelName, Action<VoiceChannelProperties> func);
    Task<IDiscordRole> CreateRoleAsync(string roleName, Color color);
    Task DeleteRoleAsync(string roleName);
    IDiscordGuildUser? GetUser(string userId);
    Task MoveAsync(IDiscordGuildUser member, IDiscordVoiceChannel channel);
    IDiscordVoiceChannel GetVoiceChannel(string channelId);
    IDiscordRole? GetRole(string roleName);
    Task<bool> CreateVoiceChannelsForCategoryAsync(string[] channelNames, string categoryId);
    Task<IDiscordRestCategoryChannel> CreateCategoryAsync(string categoryName, bool everyoneCanSee, IDiscordRole? roleToSeeChannel = null);
    IDiscordCategoryChannel? GetCategoryChannelByName(string name);
    MiniCategory? GetMiniCategory(string categoryName);
    IEnumerable<IDiscordGuildUser> GetGuildUsers(IEnumerable<string> userIds);
    IEnumerable<IDiscordGuildUser> GetInVoiceGuildUsers(IEnumerable<string> userIds);
    IEnumerable<IDiscordGuildUser> GetUsersInVoiceChannels(IEnumerable<string>? excludedChannelsIds = null);
}