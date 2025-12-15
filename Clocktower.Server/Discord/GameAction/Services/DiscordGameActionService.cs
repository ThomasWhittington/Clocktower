using Clocktower.Server.Common.Services;
using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Discord.GameAction.Services;

public class DiscordGameActionService(
    IDiscordBot bot,
    IGameStateStore gameStateStore,
    IDiscordTownManager discordTownManager,
    IDiscordConstantsService discordConstantsService,
    IUserService userService
) : IDiscordGameActionService
{
    public async Task<Result<string>> SetMuteAllPlayersAsync(string gameId, bool muted)
    {
        var (status, (guild, gameState)) = ValidateFromGameId(gameId);
        if (!status.IsSuccess) return status;

        var toBeMuted = userService.GetTownUsersForGameUsers(
            gameState.StoryTellers,
            guild.Id,
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
        return Result.Ok($"{mutedString}: {mutedPlayers}");
    }

    public async Task<Result<string>> SendToCottagesAsync(string gameId)
    {
        try
        {
            var (status, (guild, gameState)) = ValidateFromGameId(gameId);
            if (!status.IsSuccess) return status;

            var players = guild.GetGuildUsers(gameState.Players).Where(user => user.IsConnectedToVoice).ToArray();
            var storyTellers = guild.GetGuildUsers(gameState.StoryTellers).Where(user => user.IsConnectedToVoice).ToArray();

            var nightChannels = discordTownManager.GetNightChannels(guild.Id, discordConstantsService.NightCategoryName);
            var cottages = nightChannels.Select(channel => guild.GetVoiceChannel(channel.Id)).ToArray();

            if (!cottages.Any()) return Result.Fail<string>(ErrorKind.NotFound, "category.not_found", "Night channels could not be found in the town");
            if (players.Length == 0 && storyTellers.Length == 0) return Result.Ok("No users available to move");
            if (players.Length > cottages.Length - 1) return Result.Fail<string>(ErrorKind.Invalid, "channel.not_enough", "There are not enough cottages for the players");
            var storyTellersCottage = cottages[0];

            for (int i = 0; i < players.Length; i++) await players[i].MoveAsync(cottages[i + 1]);
            foreach (var storyTeller in storyTellers) await storyTeller.MoveAsync(storyTellersCottage);

            return Result.Ok("Moved players to cottages");
        }
        catch (Exception ex)
        {
            return Result.Fail<string>(ErrorKind.Unexpected, "unexpected", ex.Message);
        }
    }

    public async Task<Result<string>> SendToTownSquareAsync(string gameId)
    {
        try
        {
            var (status, (guild, _)) = ValidateFromGameId(gameId);
            if (!status.IsSuccess) return status;

            var channelId = discordTownManager.GetVoiceChannelIdByName(guild!.Id, discordConstantsService.TownSquareName);
            if (channelId is null) return Result.Fail<string>(Errors.ChannelNotFound(channelId));
            var channel = guild.GetVoiceChannel(channelId);
            if (channel is null) return Result.Fail<string>(Errors.ChannelNotFound(channelId));

            var usersInChannels = guild.GetUsersInVoiceChannels([channel.Id]).ToList();

            if (usersInChannels.Count <= 0) return Result.Ok($"No users available to move to {channel.Name}");

            foreach (var discordGuildUser in usersInChannels)
            {
                await discordGuildUser.MoveAsync(channel);
            }

            return Result.Ok($"Moved all users to {channel.Name}");
        }
        catch (Exception ex)
        {
            return Result.Fail<string>(ErrorKind.Unexpected, "unexpected", ex.Message);
        }
    }

    private (Result<string> status, (IDiscordGuild discordGuild, GameState gameState) data) ValidateFromGameId(string gameId)
    {
        var gameState = gameStateStore.Get(gameId);
        if (gameState is null) return (Result.Fail<string>(Errors.GameNotFound(gameId)), default);
        if (gameState.GuildId is null) return (Result.Fail<string>(Errors.InvalidGuildId()), default);
        var guild = bot.GetGuild(gameState.GuildId);
        if (guild is null) return (Result.Fail<string>(Errors.InvalidGuildId()), default);
        return (Result.Ok(string.Empty), (guild, gameState));
    }
}