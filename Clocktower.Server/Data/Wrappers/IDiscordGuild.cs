using Discord;

namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordGuild
{
    ulong Id { get; }
    string Name { get; }
    IEnumerable<IDiscordGuildUser> Users { get; }
    IEnumerable<IDiscordRole> Roles { get; }
    IEnumerable<IDiscordVoiceChannel> VoiceChannels { get; }
    IEnumerable<IDiscordCategoryChannel> CategoryChannels { get; }
    IDiscordRole EveryoneRole { get; }
    Task<IDiscordRestCategoryChannel> CreateCategoryChannelAsync(string categoryName, Action<GuildChannelProperties> func);
    Task<IDiscordRestVoiceChannel> CreateVoiceChannelAsync(string channelName, Action<VoiceChannelProperties> func);
    Task<IDiscordRole> CreateRoleAsync(string roleName, Color color);
    IDiscordGuildUser GetUser(ulong userId);
    Task MoveAsync(IDiscordGuildUser member, IDiscordVoiceChannel channel);
    IDiscordVoiceChannel GetVoiceChannel(ulong channelId);
}