using Clocktower.Server.Data.Dto;

namespace Clocktower.Server.Socket;

public interface IDiscordNotificationClient
{
    Task DiscordTownUpdated(DiscordTownDto discordTown);
    Task UserVoiceStateChanged(string userId, bool isInVoice, VoiceState voiceState);
    Task TownTimeChanged(string gameId, int gameTime);
    Task PingUser(string message);
    Task TimerUpdated(TimerState timer);
}