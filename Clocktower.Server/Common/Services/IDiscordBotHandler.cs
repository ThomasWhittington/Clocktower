using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public interface IDiscordBotHandler
{
    Task HandleUserVoiceStateUpdate(IDiscordUser user, IDiscordVoiceState before, IDiscordVoiceState after);
    Task UpdateTownOccupancy(IDiscordGuildUser user, IDiscordVoiceState after, string? gameId, ulong guildId);
    Task UpdateVoiceStatus(IDiscordGuildUser user, IDiscordVoiceState after, string gameId, ulong guildId);
}