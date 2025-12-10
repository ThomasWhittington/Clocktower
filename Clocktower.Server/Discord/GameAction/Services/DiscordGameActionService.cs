using Clocktower.Server.Common.Services;
using Clocktower.Server.Data.Extensions;

namespace Clocktower.Server.Discord.GameAction.Services;

public class DiscordGameActionService(
    IDiscordBot bot,
    IGameStateStore gameStateStore,
    IUserService userService
) : IDiscordGameActionService
{
    public async Task<(SetMuteAllPlayersOutcome outcome, string message)> SetMuteAllPlayersAsync(string gameId, bool muted)
    {
        var gameState = gameStateStore.Get(gameId);
        if (gameState is null) return (SetMuteAllPlayersOutcome.GameDoesNotExistError, $"Couldn't find game with id: {gameId}");
        if (!ulong.TryParse(gameState.GuildId, out var guildId)) return (SetMuteAllPlayersOutcome.InvalidGuildError, "GameState contained a guildId that is not valid");
        var guild = bot.GetGuild(guildId);
        if (guild is null) return (SetMuteAllPlayersOutcome.InvalidGuildError, "GameState contained a guildId that could not be found");


        var toBeMuted = userService.GetTownUsersForGameUsers(
            gameState.StoryTellers,
            guildId.ToString(),
            user =>
                user.IsPresent &&
                user.VoiceState.IsServerMuted == !muted
        );

        var discordUserToBeMuted = guild.GetGuildUsers(toBeMuted.GetIds());

        var mutedPlayers = 0;

        foreach (var discordUser in discordUserToBeMuted)
        {
            await discordUser.SetIsServerMuted(muted);
            mutedPlayers++;
        }

        var mutedString = muted ? "Muted" : "UnMuted";
        return (SetMuteAllPlayersOutcome.PlayersUpdated, $"{mutedString}: {mutedPlayers}");
    }
}