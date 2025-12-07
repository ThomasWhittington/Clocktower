using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public interface IDiscordBotHandler
{
    Task HandleUserVoiceStateUpdate(IDiscordUser user, IDiscordVoiceState before, IDiscordVoiceState after);
    Task UpdateTownOccupancy(IDiscordGuildUser user, IDiscordVoiceState before, IDiscordVoiceState after, string? gameId, ulong guildId);
    Task UpdateMutedStatus(IDiscordGuildUser user, IDiscordVoiceState before, IDiscordVoiceState after, string gameId);
}