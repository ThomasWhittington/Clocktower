using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Socket;

namespace Clocktower.Server.Common.Services;

public class DiscordBotHandler(
    IGameStateStore gameStateStore,
    ITownOccupantManager townOccupantManager,
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
        var gameState = gameStateStore.GetGuildGames(guildId.Value).FirstOrDefault();

        await UpdateTownOccupancy(guildUser, before, after, gameState?.Id, guildId.Value);

        if (gameState is null) return;

        await UpdateMutedStatus(guildUser, before, after, gameState.Id);
    }

    public virtual async Task UpdateTownOccupancy(IDiscordGuildUser user, IDiscordVoiceState before, IDiscordVoiceState after, string? gameId, ulong guildId)
    {
        var channelsAreSame = before.VoiceChannel?.Id == after.VoiceChannel?.Id;
        if (channelsAreSame) return;

        using var scope = serviceScopeFactory.CreateScope();
        var townService = scope.ServiceProvider.GetRequiredService<IDiscordTownService>();
        var (success, thisTownOccupancy, _) = await townService.GetTownOccupancy(guildId);
        if (!success || thisTownOccupancy is null) return;
        var newTownOccupancy = townOccupantManager.MoveUser(thisTownOccupancy, user, after.VoiceChannel);

        if (gameId is not null)
        {
            await notificationService.BroadcastTownOccupancyUpdate(gameId, newTownOccupancy);
        }
    }

    public virtual async Task UpdateMutedStatus(IDiscordGuildUser user, IDiscordVoiceState before, IDiscordVoiceState after, string gameId)
    {
        bool inVoice = after.VoiceChannel != null;
        var discordMutedState = new MutedState(after.IsMuted, after.IsDeafened, after.IsSelfMuted, after.IsSelfDeafened);

        gameStateStore.UpdateUser(gameId, user.Id, isPresent: inVoice, discordMutedState: discordMutedState);

        await notificationService.BroadcastUserVoiceStateChanged(gameId, user.Id.ToString(), inVoice, discordMutedState);
    }
}