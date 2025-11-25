using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordGuild(SocketGuild guild) : IDiscordGuild
{
    public ulong Id => guild.Id;
    public string Name => guild.Name;
    public IEnumerable<IDiscordGuildUser> Users => guild.Users.Select(user => new DiscordGuildUser(user));
    public IEnumerable<IDiscordRole> Roles => guild.Roles.Select(role => new DiscordRole(role));
    public IEnumerable<IDiscordVoiceChannel> VoiceChannels => guild.VoiceChannels.Select(channel => new DiscordVoiceChannel(channel));
    public IEnumerable<IDiscordCategoryChannel> CategoryChannels => guild.CategoryChannels.Select(channel => new DiscordCategoryChannel(channel));
    public IDiscordRole EveryoneRole => new DiscordRole(guild.EveryoneRole);

    public async Task<IDiscordRestCategoryChannel> CreateCategoryChannelAsync(string categoryName, Action<GuildChannelProperties> func)
    {
        return new DiscordRestCategoryChannel(await guild.CreateCategoryChannelAsync(categoryName, func));
    }

    public async Task<IDiscordRestVoiceChannel> CreateVoiceChannelAsync(string channelName, Action<VoiceChannelProperties> func)
    {
        return new DiscordRestVoiceChannel(await guild.CreateVoiceChannelAsync(channelName, func));
    }

    public async Task<IDiscordRole> CreateRoleAsync(string roleName, Color color)
    {
        return new DiscordRole(await guild.CreateRoleAsync(roleName, color: color));
    }

    public IDiscordGuildUser GetUser(ulong userId)
    {
        return new DiscordGuildUser(guild.GetUser(userId));
    }

    public async Task MoveAsync(IDiscordGuildUser member, IDiscordVoiceChannel channel)
    {
        await member.MoveAsync(channel);
    }

    public IDiscordVoiceChannel GetVoiceChannel(ulong channelId)
    {
        return new DiscordVoiceChannel(guild.GetVoiceChannel(channelId));
    }
}