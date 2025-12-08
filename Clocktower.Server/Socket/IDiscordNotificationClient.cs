namespace Clocktower.Server.Socket;

public interface IDiscordNotificationClient
{
    Task TownOccupancyUpdated(TownOccupants townOccupants);
    Task UserVoiceStateChanged(string userId, bool isInVoice, VoiceState voiceState);
    Task TownTimeChanged(int gameTime);
    Task PingUser(string message);
}