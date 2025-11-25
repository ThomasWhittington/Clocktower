using System.Diagnostics.CodeAnalysis;
using Discord;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordUser(IUser user) : IDiscordUser
{
    public ulong Id => user.Id;
    public string GlobalName => user.GlobalName;
    public string DisplayAvatarUrl => user.GetDisplayAvatarUrl();

    public async Task<IDiscordDmChannel?> CreateDmChannelAsync()
    {
        var dmChannel = await user.CreateDMChannelAsync();
        return dmChannel != null ? new DiscordDmChannel(dmChannel) : null;
    }
}