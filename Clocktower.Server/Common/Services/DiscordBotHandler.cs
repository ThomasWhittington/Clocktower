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

    /// <summary>
    /// Move a user's voice membership within the guild's Discord town and broadcast a town update for the specified game.
    /// </summary>
    /// <param name="user">The guild user whose voice membership should be moved.</param>
    /// <param name="after">The user's new voice state (may contain the target voice channel).</param>
    /// <param name="gameId">The identifier of the game whose Discord town should be updated.</param>
    /// <param name="guildId">The Discord guild identifier where the town resides.</param>
    /// <returns>A task that completes when the town has been updated and the notification has been broadcast.</returns>
    public virtual async Task UpdateDiscordTown(IDiscordGuildUser user, IDiscordVoiceState after, string gameId, string guildId)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var townService = scope.ServiceProvider.GetRequiredService<IDiscordTownService>();
        var (success, discordTown, _) = await townService.GetDiscordTown(guildId);
        if (!success || discordTown is null) return;
        discordDiscordTownManager.MoveUser(discordTown, user, after.VoiceChannel);

        await notificationService.BroadcastDiscordTownUpdate(gameId);
    }

    /// <summary>
    /// Update the user's Discord voice presence and broadcast a Discord-town update for the specified game.
    /// </summary>
    /// <param name="user">The guild-scoped Discord user whose presence will be updated.</param>
    /// <param name="after">The new voice state observed for the user.</param>
    /// <param name="gameId">The identifier of the game for which to broadcast the Discord-town update.</param>
    /// <param name="guildId">The identifier of the Discord guild where the presence change occurred.</param>
    public virtual async Task UpdateVoiceStatus(IDiscordGuildUser user, IDiscordVoiceState after, string gameId, string guildId)
    {
        bool inVoice = after.VoiceChannel != null;
        var discordVoiceState = new VoiceState(after.IsMuted, after.IsDeafened, after.IsSelfMuted, after.IsSelfDeafened);
        userService.UpdateDiscordPresence(user.Id, guildId, inVoice, discordVoiceState);

        await notificationService.BroadcastDiscordTownUpdate(gameId);
    }
}