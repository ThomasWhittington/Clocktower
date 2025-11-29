namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordCategoryChannel
{
    ulong Id { get; }
    string Name { get; }
    IEnumerable<IDiscordChannel> Channels { get; }
    IEnumerable<IDiscordVoiceChannel> VoiceChannels { get; }
    Task DeleteAsync();
    bool VerifyCategoryChannels(string[] channelNames);
    IEnumerable<ChannelOccupants> GetChannelOccupancy();
}