using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Socket;

namespace Clocktower.Server.Common.Services;

public class DiscordBotHandler(
    IGameStateStore gameStateStore,
    IDiscordTownManager discordDiscordTownManager,
    IUserService userService,
    INotificationService notificationService,
    IServiceScopeFactory serviceScopeFactory
) : IDiscordBotHandler
{
    public async Task HandleUserVoiceStateUpdate(IDiscordUser user, IDiscordVoiceState before, IDiscordVoiceState after)
    {
        var guildId = after.GuildId ?? before.GuildId;
        if (guildId is null) return;
        var guildUser = user.GetGuildUser();
        if (guildUser is null) return;
        var gameStates = gameStateStore.GetGuildGames(guildId);

        var channelsAreSame = before.VoiceChannel?.Id == after.VoiceChannel?.Id;
        if (channelsAreSame)
        {
            foreach (var gameState in gameStates)
            {
                await UpdateVoiceStatus(guildUser, after, gameState.Id, guildId);
            }
        }
        else
        {
            foreach (var gameState in gameStates)
            {
                await UpdateDiscordTown(guildUser, after, gameState.Id, guildId);
            }
        }
    }

    public virtual async Task UpdateDiscordTown(IDiscordGuildUser user, IDiscordVoiceState after, string gameId, string guildId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var townService = scope.ServiceProvider.GetRequiredService<IDiscordTownService>();
        var (success, discordTown, _) = await townService.GetDiscordTown(guildId);
        if (!success || discordTown is null) return;
        discordDiscordTownManager.MoveUser(discordTown, user, after.VoiceChannel);

        await notificationService.BroadcastDiscordTownUpdate(gameId);
    }

    public virtual async Task UpdateVoiceStatus(IDiscordGuildUser user, IDiscordVoiceState after, string gameId, string guildId)
    {
        bool inVoice = after.VoiceChannel != null;
        var discordVoiceState = new VoiceState(after.IsMuted, after.IsDeafened, after.IsSelfMuted, after.IsSelfDeafened);
        userService.UpdateDiscordPresence(user.Id, guildId, inVoice, discordVoiceState);

        await notificationService.BroadcastDiscordTownUpdate(gameId);
    }
}