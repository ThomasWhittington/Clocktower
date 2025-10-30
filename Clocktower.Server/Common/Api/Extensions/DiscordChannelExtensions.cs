using DSharpPlus.Entities;

namespace Clocktower.Server.Common.Api.Extensions;

public static class DiscordChannelExtensions
{
    public static async Task DeleteCategoryAsync(this DiscordChannel categoryChannel)
    {
        if (categoryChannel is null) return;

        foreach (var channel in categoryChannel.Children)
        {
            await channel.DeleteAsync();
        }

        await categoryChannel.DeleteAsync();
    }
}