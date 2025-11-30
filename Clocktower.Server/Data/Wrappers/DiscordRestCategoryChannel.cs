using System.Diagnostics.CodeAnalysis;
using Discord.Rest;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordRestCategoryChannel(RestCategoryChannel channel) : IDiscordRestCategoryChannel
{
    public ulong Id => channel.Id;
}