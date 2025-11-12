namespace Clocktower.Server.Socket;

public interface INotificationService
{
    Task BroadcastTownOccupancyUpdate(TownOccupants occupants);
    Task BroadcastUserVoiceStateChanged(string userId, bool inVoice);
    Task BroadcastTownTime(GameTime gameTime);
}