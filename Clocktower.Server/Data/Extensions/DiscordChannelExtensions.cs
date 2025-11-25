using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Data.Extensions;

public static class DiscordChannelExtensions
{
    extension(IDiscordCategoryChannel categoryChannel)
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
            if (categoryChannel.Channels.Count() != channelNames.Length) return false;
            return channelNames.All(channelName => categoryChannel.Channels.Any(o => o.Name == channelName));
        }
    }
}