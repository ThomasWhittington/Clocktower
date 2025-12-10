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
        if (!guildId.HasValue) return;
        var guildUser = user.GetGuildUser();
        if (guildUser is null) return;
        var gameStates = gameStateStore.GetGuildGames(guildId.Value);

        var channelsAreSame = before.VoiceChannel?.Id == after.VoiceChannel?.Id;
        if (channelsAreSame)
        {
            foreach (var gameState in gameStates)
            {
                await UpdateVoiceStatus(guildUser, after, gameState.Id, guildId.Value);
            }
        }
        else
        {
            foreach (var gameState in gameStates)
            {
                await UpdateDiscordTown(guildUser, after, gameState.Id, guildId.Value);
            }
        }
    }

    public virtual async Task UpdateDiscordTown(IDiscordGuildUser user, IDiscordVoiceState after, string gameId, ulong guildId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var townService = scope.ServiceProvider.GetRequiredService<IDiscordTownService>();
        var (success, discordTown, _) = await townService.GetDiscordTown(guildId);
        if (!success || discordTown is null) return;
        var newDiscordTown = discordDiscordTownManager.MoveUser(discordTown, user, after.VoiceChannel);

        await notificationService.BroadcastDiscordTownUpdate(gameId, newDiscordTown);
    }

    public virtual async Task UpdateVoiceStatus(IDiscordGuildUser user, IDiscordVoiceState after, string gameId, ulong guildId)
    {
        bool inVoice = after.VoiceChannel != null;
        var discordVoiceState = new VoiceState(after.IsMuted, after.IsDeafened, after.IsSelfMuted, after.IsSelfDeafened);

        userService.UpdateDiscordPresence(user.Id.ToString(), guildId.ToString(), inVoice, discordVoiceState);

        var discordTown = discordDiscordTownManager.GetDiscordTown(guildId);
        if (discordTown is not null)
        {
            await notificationService.BroadcastDiscordTownUpdate(gameId, discordTown);
        }
    }
}