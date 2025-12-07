namespace Clocktower.Server.Socket;

public interface IDiscordNotificationClient
{
    Task TownOccupancyUpdated(TownOccupants townOccupants);
    Task UserVoiceStateChanged(string userId, bool isInVoice, MutedState mutedState);
    Task TownTimeChanged(int gameTime);
    Task PingUser(string message);
}