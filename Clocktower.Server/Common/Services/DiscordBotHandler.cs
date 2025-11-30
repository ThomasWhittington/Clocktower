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
        var channelsAreSame = before.VoiceChannel?.Id == after.VoiceChannel?.Id;
        if (!guildId.HasValue || channelsAreSame) return;
        
        using var scope = serviceScopeFactory.CreateScope();
        var townService = scope.ServiceProvider.GetRequiredService<IDiscordTownService>();
        var (success, thisTownOccupancy, _) = await townService.GetTownOccupancy(guildId.Value);
        if (!success || thisTownOccupancy is null) return;
        var newTownOccupancy = townOccupantManager.MoveUser(thisTownOccupancy, user, after.VoiceChannel);

        var gameState = gameStateStore.GetGuildGames(guildId.Value).FirstOrDefault();
        if (gameState is not null)
        {
            await notificationService.BroadcastTownOccupancyUpdate(gameState.Id, newTownOccupancy);
            await notificationService.BroadcastUserVoiceStateChanged(gameState.Id, user.Id.ToString(), after.VoiceChannel != null);
        }
    }
}