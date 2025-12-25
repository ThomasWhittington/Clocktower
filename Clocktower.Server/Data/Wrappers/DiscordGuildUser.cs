using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordGuildUser(SocketGuildUser user) : IDiscordGuildUser
{
    public string Id => user.Id.ToString();
    public string GuildId => user.Guild.Id.ToString();
    public string DisplayName => user.DisplayName;
    public string DisplayAvatarUrl => user.GetDisplayAvatarUrl();
    public bool IsServerMuted => user.IsMuted;
    public bool IsServerDeafened => user.IsDeafened;
    public bool IsSelfMuted => user.IsSelfMuted;
    public bool IsSelfDeafened => user.IsSelfDeafened;
    public bool IsConnectedToVoice => VoiceState?.VoiceChannel != null;
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
        if (!ulong.TryParse(channel.Id, out var id))
            throw new ArgumentException($"Invalid channel ID format: {channel.Id}", nameof(channel));
        await user.ModifyAsync(x => x.ChannelId = id);
    }

    public async Task RemoveRoleAsync(IDiscordRole role)
    {
        if (!ulong.TryParse(role.Id, out var id))
            throw new ArgumentException($"Invalid role ID format: {role.Id}", nameof(role));
        await user.RemoveRoleAsync(id);
    }

    public async Task AddRoleAsync(IDiscordRole role)
    {
        if (!ulong.TryParse(role.Id, out var id))
            throw new ArgumentException($"Invalid role ID format: {role.Id}", nameof(role));
        await user.AddRoleAsync(id);
    }

    public bool DoesUserHaveRole(string roleId)
    {
        return Roles.Any(o => o.Id == roleId);
    }

    public async Task SetIsServerMuted(bool isMuted)
    {
        await user.ModifyAsync(o => o.Mute = isMuted);
    }

    public GameUser AsGameUser(GamePerspective? gamePerspective = null)
    {
        var result = new GameUser(user.Id.ToString());
        if (gamePerspective is not null)
        {
            result.UserType = gamePerspective.GetUserType(user.Id.ToString());
        }

        return result;
    }

    public TownUser AsTownUser()
    {
        var result = new TownUser(user.Id.ToString(), user.DisplayName, DisplayAvatarUrl)
        {
            VoiceState = new VoiceState(IsServerMuted, IsServerDeafened, IsSelfMuted, IsSelfDeafened),
            IsPresent = VoiceState?.VoiceChannel != null
        };

        return result;
    }
}