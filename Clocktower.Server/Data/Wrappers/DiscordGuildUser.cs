using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordGuildUser(SocketGuildUser user) : IDiscordGuildUser
{
    public ulong Id => user.Id;
    public string DisplayName => user.DisplayName;
    public string DisplayAvatarUrl => user.GetDisplayAvatarUrl();
    public IDiscordVoiceState? VoiceState => user.VoiceState.HasValue ? new DiscordVoiceState(user.VoiceState.Value) : null;
    public IEnumerable<IDiscordRole> Roles => user.Roles.Select(r => new DiscordRole(r));

    public bool IsAdministrator()
    {
        return user.GuildPermissions.Administrator;
    }

    public async Task<IDiscordDmChannel?> CreateDmChannelAsync()
    {
        var dmChannel = await user.CreateDMChannelAsync();
        return dmChannel != null ? new DiscordDmChannel(dmChannel) : null;
    }

    public async Task MoveAsync(IDiscordVoiceChannel channel)
    {
        await user.ModifyAsync(x => x.ChannelId = channel.Id);
    }

    public async Task RemoveRoleAsync(IDiscordRole role)
    {
        await user.RemoveRoleAsync(role.Id);
    }

    public async Task AddRoleAsync(IDiscordRole role)
    {
        await user.AddRoleAsync(role.Id);
    }

    public bool DoesUserHaveRole(ulong roleId)
    {
       return Roles.Any(o => o.Id == roleId);
    }
}