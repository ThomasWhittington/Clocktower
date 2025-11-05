using Clocktower.Server.Common;
using DSharpPlus;

namespace Clocktower.Server.DiscordTown.Services;

public class DiscordBotService(Secrets secrets)
{
    public DiscordClient Client { get; set; } = new(new DiscordConfiguration { TokenType = TokenType.Bot, Token = secrets.DiscordBotToken });

    public async Task InitializeAsync()
    {
        await Client.ConnectAsync();
    }
}