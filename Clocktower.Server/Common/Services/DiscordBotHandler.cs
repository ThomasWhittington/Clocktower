using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Socket;

namespace Clocktower.Server.Common.Services;

public class DiscordBotHandler(IGameStateStore gameStateStore, ITownOccupantManager townOccupantManager, INotificationService notificationService, IServiceProvider serviceProvider) : IDiscordBotHandler
{
    public async Task HandleUserVoiceStateUpdate(IDiscordUser user, IDiscordVoiceState before, IDiscordVoiceState after)
    {
        var guildId = after.VoiceChannel?.Guild.Id ?? before.VoiceChannel?.Guild.Id;
        var channelsAreSame = before.VoiceChannel?.Id == after.VoiceChannel?.Id;
        if (!guildId.HasValue || channelsAreSame) return;

        var gameState = gameStateStore.GetGuildGames(guildId.Value).FirstOrDefault();
        if (gameState is null) return;

        using var scope = serviceProvider.CreateScope();
        var townService = scope.ServiceProvider.GetRequiredService<IDiscordTownService>();
        var (success, thisTownOccupancy, _) = await townService.GetTownOccupancy(guildId.Value);
        if (!success || thisTownOccupancy is null) return;
        var newTownOccupancy = townOccupantManager.MoveUser(thisTownOccupancy, user, after.VoiceChannel);
        await notificationService.BroadcastTownOccupancyUpdate(gameState.Id, newTownOccupancy);
        await notificationService.BroadcastUserVoiceStateChanged(gameState.Id, user.Id.ToString(), after.VoiceChannel != null);
    }
}