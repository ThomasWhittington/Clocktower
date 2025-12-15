using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordGuild(SocketGuild guild) : IDiscordGuild
{
    public string Id => guild.Id.ToString();
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

    public async Task DeleteRoleAsync(string roleName)
    {
        var role = GetRole(roleName);
        if (role is not null)
        {
            await role.DeleteAsync();
        }
    }

    public IDiscordGuildUser? GetUser(string userId)
    {
        var id = ulong.Parse(userId);
        var user = guild.GetUser(id);
        return user != null ? new DiscordGuildUser(user) : null;
    }

    public async Task MoveAsync(IDiscordGuildUser member, IDiscordVoiceChannel channel)
    {
        await member.MoveAsync(channel);
    }

    public IDiscordVoiceChannel GetVoiceChannel(string channelId)
    {
        var id = ulong.Parse(channelId);
        return new DiscordVoiceChannel(guild.GetVoiceChannel(id));
    }

    public IDiscordRole? GetRole(string roleName)
    {
        var role = guild.Roles.FirstOrDefault(o => o.Name == roleName);
        return role != null ? new DiscordRole(role) : null;
    }

    public async Task<bool> CreateVoiceChannelsForCategoryAsync(string[] channelNames, string categoryId)
    {
        var id = ulong.Parse(categoryId);
        foreach (var channelName in channelNames)
        {
            var result = await guild.CreateVoiceChannelAsync(channelName, properties => properties.CategoryId = id);
            if (result is null) return false;
        }

        return true;
    }


    public async Task<IDiscordRestCategoryChannel> CreateCategoryAsync(string categoryName, bool everyoneCanSee, IDiscordRole? roleToSeeChannel = null)
    {
        IEnumerable<Overwrite> permissions;

        if (everyoneCanSee)
        {
            permissions =
            [
                new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role,
                    new OverwritePermissions(viewChannel: PermValue.Allow)
                )
            ];
        }
        else
        {
            permissions =
            [
                new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role,
                    new OverwritePermissions(viewChannel: PermValue.Deny)
                )
            ];

            if (roleToSeeChannel != null)
            {
                var id = ulong.Parse(roleToSeeChannel.Id);
                permissions = permissions.Append(new Overwrite(id, PermissionTarget.Role,
                    new OverwritePermissions(viewChannel: PermValue.Allow)
                ));
            }
        }

        var category = await guild.CreateCategoryChannelAsync(categoryName, properties => { properties.PermissionOverwrites = new Optional<IEnumerable<Overwrite>>(permissions); });

        return new DiscordRestCategoryChannel(category);
    }

    public IDiscordCategoryChannel? GetCategoryChannelByName(string name)
    {
        var categoryChannel = guild.CategoryChannels.FirstOrDefault(o => o.Name == name);
        return categoryChannel != null ? new DiscordCategoryChannel(categoryChannel) : null;
    }

    public MiniCategory? GetMiniCategory(string categoryName)
    {
        var categoryChannel = GetCategoryChannelByName(categoryName);
        if (categoryChannel == null) return null;
        var miniCategory = new MiniCategory(categoryChannel.Id, categoryChannel.Name, categoryChannel.GetChannelOccupancy());
        return miniCategory;
    }

    public IEnumerable<IDiscordGuildUser> GetGuildUsers(IEnumerable<string> userIds)
    {
        return Users.Where(o => userIds.Contains(o.Id.ToString()));
    }

    public IEnumerable<IDiscordGuildUser> GetGuildUsers(IEnumerable<IIdentifiable> identifiables)
    {
        return GetGuildUsers(identifiables.Select(o => o.Id));
    }

    public IEnumerable<IDiscordGuildUser> GetUsersInVoiceChannels(List<string>? excludedChannelsIds = null)
    {
        excludedChannelsIds ??= [];
        return VoiceChannels.Where(voiceChannel => !excludedChannelsIds.Contains(voiceChannel.Id.ToString()))
            .SelectMany(voiceChannel => voiceChannel.ConnectedUsers);
    }
}