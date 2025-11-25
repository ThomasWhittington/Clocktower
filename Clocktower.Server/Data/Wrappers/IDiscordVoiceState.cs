namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordVoiceState
{
    IDiscordVoiceChannel? VoiceChannel { get; }
}