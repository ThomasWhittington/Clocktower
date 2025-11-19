using Discord.WebSocket;

namespace Clocktower.Server.Common.Api.Extensions;

public static class DiscordChannelExtensions
{
    extension(SocketCategoryChannel categoryChannel)
    {
        public async Task DeleteCategoryAsync()
        {
            if (categoryChannel is null) return;

            foreach (var channel in categoryChannel.Channels)
            {
                await channel.DeleteAsync();
            }

            await categoryChannel.DeleteAsync();
        }

        public bool VerifyCategoryChannels(string[] channelNames)
        {
            if (categoryChannel.Channels.Count != channelNames.Length) return false;
            return channelNames.All(channelName => categoryChannel.Channels.Any(o => o.Name == channelName));
        }
    }
}