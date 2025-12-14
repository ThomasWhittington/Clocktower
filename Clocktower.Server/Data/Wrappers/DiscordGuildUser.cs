using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordGuildUser(SocketGuildUser user) : IDiscordGuildUser
{
    public ulong Id => user.Id;
    public ulong GuildId => user.Guild.Id;
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

    public async Task SetIsServerMuted(bool isMuted)
    {
        await user.ModifyAsync(o => o.Mute = isMuted);
    }

    public GameUser AsGameUser(GameState? gameState = null)
    {
        var result = new GameUser(user.Id.ToString());
        if (gameState is not null)
        {
            result.UserType = gameState.GetUserType(user.Id.ToString());
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