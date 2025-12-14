namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordGuildUser
{
    ulong Id { get; }
    ulong GuildId { get; }
    string DisplayName { get; }
    string DisplayAvatarUrl { get; }
    public bool IsServerMuted { get; }
    public bool IsServerDeafened { get; }
    public bool IsSelfMuted { get; }
    public bool IsSelfDeafened { get; }
    public bool IsConnectedToVoice { get; }
    IDiscordVoiceState? VoiceState { get; }
    IEnumerable<IDiscordRole> Roles { get; }
    bool IsAdministrator();
    Task<IDiscordDmChannel?> CreateDmChannelAsync();
    Task MoveAsync(IDiscordVoiceChannel channel);
    Task RemoveRoleAsync(IDiscordRole role);
    Task AddRoleAsync(IDiscordRole role);
    bool DoesUserHaveRole(ulong roleId);
    Task SetIsServerMuted(bool isMuted);
    GameUser AsGameUser(GameState? gameState = null);
    TownUser AsTownUser();
}