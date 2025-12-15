using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordCategoryChannel(SocketCategoryChannel channel) : IDiscordCategoryChannel
{
    public string Id => channel.Id.ToString();
    public string Name => channel.Name;
    public IEnumerable<IDiscordChannel> Channels => channel.Channels.Select(c => new DiscordChannel(c));

    public IEnumerable<IDiscordVoiceChannel> VoiceChannels => channel.Channels.Where(o => o.ChannelType == ChannelType.Voice).Cast<SocketVoiceChannel>()
        .Select(c => new DiscordVoiceChannel(c));

    public async Task DeleteAsync()
    {
        foreach (var containedChannel in channel.Channels)
        {
            await containedChannel.DeleteAsync();
        }

        await channel.DeleteAsync();
    }

    public bool VerifyCategoryChannels(string[] channelNames)
    {
        if (channel.Channels.Count != channelNames.Length) return false;
        return channelNames.All(channelName => channel.Channels.Any(o => o.Name == channelName));
    }

    public IEnumerable<ChannelOccupants> GetChannelOccupancy()
    {
        var channels = VoiceChannels.OrderBy(o => o.Position);
        return (from discordChannel in channels
            let miniChannel = new MiniChannel(discordChannel.Id, discordChannel.Name)
            let occupants = discordChannel.ConnectedUsers.Select(discordChannelUser => discordChannelUser.AsTownUser()).ToList()
            select new ChannelOccupants(miniChannel, occupants)).ToList();
    }
}