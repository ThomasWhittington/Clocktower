using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Socket.Services;

namespace Clocktower.Server.Common.Services;

public class DiscordBotHandler(
    IGamePerspectiveService gamePerspectiveService,
    IDiscordTownManager discordDiscordTownManager,
    IUserService userService,
    IGameBroadcastService gameBroadcastService,
    ITalkRequestManager talkRequestManager,
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
            await RemoveTalkRequestsForUsersInSameChannel(gameId, guildId);
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

    private async Task RemoveTalkRequestsForUsersInSameChannel(string gameId, string guildId)
    {
        var discordTown = discordDiscordTownManager.GetDiscordTown(guildId);
        if (discordTown is null) return;

        var talkRequests = talkRequestManager.GetTalkRequests(gameId);

        foreach (var request in talkRequests)
        {
            var requesterChannel = discordDiscordTownManager.FindUserChannel(discordTown, request.RequesterId);
            var targetChannel = discordDiscordTownManager.FindUserChannel(discordTown, request.TargetId);

            if (requesterChannel is not null &&
                targetChannel is not null &&
                requesterChannel.Channel.Id == targetChannel.Channel.Id)
            {
                await talkRequestManager.RemoveTalkRequest(gameId, request.RequesterId, request.TargetId);
            }
        }
    }
}