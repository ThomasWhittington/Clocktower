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

        var channelsAreSame = before.VoiceChannel?.Id == after.VoiceChannel?.Id;
        if (channelsAreSame)
        {
            if (gameState is null) return;
            await UpdateVoiceStatus(guildUser, after, gameState.Id, guildId.Value);
        }
        else
        {
            await UpdateTownOccupancy(guildUser, after, gameState?.Id, guildId.Value);
        }
    }

    public virtual async Task UpdateTownOccupancy(IDiscordGuildUser user, IDiscordVoiceState after, string? gameId, ulong guildId)
    {
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

    public virtual async Task UpdateVoiceStatus(IDiscordGuildUser user, IDiscordVoiceState after, string gameId, ulong guildId)
    {
        bool inVoice = after.VoiceChannel != null;
        var discordVoiceState = new VoiceState(after.IsMuted, after.IsDeafened, after.IsSelfMuted, after.IsSelfDeafened);

        gameStateStore.UpdateUser(gameId, user.Id, isPresent: inVoice, voiceState: discordVoiceState);

        var townOccupants = townOccupantManager.UpdateUserStatus(guildId, user.Id, inVoice, discordVoiceState);
        if (townOccupants is not null)
        {
            await notificationService.BroadcastTownOccupancyUpdate(gameId, townOccupants);
        }
    }
}