using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Socket.Services;

namespace Clocktower.Server.Common.Services;

public class DiscordBotHandler(
    IGamePerspectiveService gamePerspectiveService,
    IDiscordTownManager discordDiscordTownManager,
    IUserService userService,
    IGameBroadcastService gameBroadcastService,
    IServiceScopeFactory serviceScopeFactory
) : IDiscordBotHandler
{
    public async Task HandleUserVoiceStateUpdate(IDiscordUser user, IDiscordVoiceState before, IDiscordVoiceState after)
    {
        var guildId = after.GuildId ?? before.GuildId;
        if (guildId is null) return;
        var guildUser = user.GetGuildUser();
        if (guildUser is null) return;
        var guildGameIds = gamePerspectiveService.GetGuildGameIds(guildId).ToArray();
        if (!guildGameIds.Any()) return;

        bool inVoice = after.VoiceChannel != null;
        var discordVoiceState = new VoiceState(inVoice, after.IsMuted, after.IsDeafened, after.IsSelfMuted, after.IsSelfDeafened);

        var channelsAreSame = before.VoiceChannel?.Id == after.VoiceChannel?.Id;
        if (channelsAreSame || after.VoiceChannel == null)
        {
            userService.UpdateDiscordPresence(guildUser.Id, guildId, discordVoiceState);
        }
        else
        {
            await UpdateDiscordTown(guildUser, after, guildId, discordVoiceState);
        }

        foreach (var gameId in guildGameIds)
        {
            await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
        }
    }

    public virtual async Task UpdateDiscordTown(IDiscordGuildUser user, IDiscordVoiceState after, string guildId, VoiceState voiceState)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var townService = scope.ServiceProvider.GetRequiredService<IDiscordTownService>();
        var (success, discordTown, _) = await townService.GetDiscordTown(guildId);
        if (!success || discordTown is null) return;
        discordDiscordTownManager.MoveUser(discordTown, user, after.VoiceChannel, voiceState);
    }
}