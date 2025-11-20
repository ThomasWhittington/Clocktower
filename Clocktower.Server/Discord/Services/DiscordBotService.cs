using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Socket;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Clocktower.Server.Discord.Services;

public class DiscordBotService(IOptions<Secrets> secretsOptions, IGameStateStore gameStateStore, IServiceProvider serviceProvider, INotificationService notificationService)
    : BackgroundService, IDiscordBotService
{
    private readonly Secrets _secrets = secretsOptions.Value;

    public DiscordSocketClient Client { get; } = new(new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.All
    });

    public IUser GetUser(ulong userId)
    {
        return Client.GetUser(userId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.Log += LogAsync;
        Client.Ready += ReadyAsync;

        Client.UserVoiceStateUpdated += ClientOnUserVoiceStateUpdated;

        string token = _secrets.DiscordBotToken;
        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ClientOnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        var guildId = after.VoiceChannel?.Guild?.Id ?? before.VoiceChannel?.Guild?.Id;
        if (guildId.HasValue && before.VoiceChannel?.Id != after.VoiceChannel?.Id)
        {
            var gameState = gameStateStore.GetGuildGames(guildId.Value).FirstOrDefault();
            if (gameState is null) return;

            using var scope = serviceProvider.CreateScope();
            var townService = scope.ServiceProvider.GetRequiredService<IDiscordTownService>();
            var (success, thisTownOccupancy, _) = await townService.GetTownOccupancy(guildId.Value);
            if (!success) return;
            thisTownOccupancy!.MoveUser(user, after);
            await notificationService.BroadcastTownOccupancyUpdate(gameState.Id, thisTownOccupancy);
            await notificationService.BroadcastUserVoiceStateChanged(gameState.Id, user.Id.ToString(), after.VoiceChannel != null);
        }
    }


    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        Console.WriteLine($"{Client.CurrentUser} is connected!");
        return Task.CompletedTask;
    }
}