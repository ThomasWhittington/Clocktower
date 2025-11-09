using Discord.WebSocket;

namespace Clocktower.Server.Common.Api.Extensions;

public static class DiscordChannelExtensions
{
    public static async Task DeleteCategoryAsync(this SocketCategoryChannel categoryChannel)
    {
        if (categoryChannel is null) return;

        foreach (var channel in categoryChannel.Channels)
        {
            await channel.DeleteAsync();
        }

        await categoryChannel.DeleteAsync();
    }
}