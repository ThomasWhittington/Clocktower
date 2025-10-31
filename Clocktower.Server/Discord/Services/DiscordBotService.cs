using Clocktower.Server.Common;
using DSharpPlus;

namespace Clocktower.Server.Discord.Services;

public class DiscordBotService(Secrets secrets)
{
    public DiscordClient Client { get; set; } = new(new DiscordConfiguration { TokenType = TokenType.Bot, Token = secrets.DiscordBotToken });

    public async Task InitializeAsync()
    {
        Client.VoiceStateUpdated += (_, args) =>
        {
            if (args.Before?.Channel?.Id != args.After?.Channel?.Id)
            {
                var user = args.User;
                var beforeChannel = args.Before?.Channel;
                var afterChannel = args.After?.Channel;
                Console.WriteLine($"{user.Username}: {beforeChannel?.Name} -> {afterChannel?.Name}");
            }

            return Task.CompletedTask;
        };

        await Client.ConnectAsync();
    }
}