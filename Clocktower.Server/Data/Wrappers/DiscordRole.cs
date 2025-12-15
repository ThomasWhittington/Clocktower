using System.Diagnostics.CodeAnalysis;
using Discord;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordRole(IRole role) : IDiscordRole
{
    public string Id => role.Id.ToString();
    public string Name => role.Name;

    public async Task DeleteAsync()
    {
        await role.DeleteAsync();
    }
}