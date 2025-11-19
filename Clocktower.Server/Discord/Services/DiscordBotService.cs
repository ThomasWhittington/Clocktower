using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Socket;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Clocktower.Server.Discord.Services;

public class DiscordBotService : BackgroundService, IDiscordBotService
{
    private readonly Secrets _secrets;
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationService _notificationService;

    public DiscordBotService(IOptions<Secrets> secretsOptions, IServiceProvider serviceProvider, INotificationService notificationService)
    {
        _serviceProvider = serviceProvider;
        _notificationService = notificationService;
        _secrets = secretsOptions.Value;

        if (string.IsNullOrEmpty(_secrets.ServerUri))
        {
            throw new ArgumentNullException(nameof(secretsOptions), "Secrets are not set");
        }
    }

    public DiscordSocketClient Client { get; } = new(new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.All
    });

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
            var gameState = GameStateStore.GetGames(guildId.Value).FirstOrDefault();
            if (gameState is null) return;

            using var scope = _serviceProvider.CreateScope();
            var townService = scope.ServiceProvider.GetRequiredService<IDiscordTownService>();
            var (success, thisTownOccupancy, _) = await townService.GetTownOccupancy(guildId.Value);
            if (!success) return;
            thisTownOccupancy!.MoveUser(user, after);
            await _notificationService.BroadcastTownOccupancyUpdate(gameState.Id, thisTownOccupancy);
            await _notificationService.BroadcastUserVoiceStateChanged(gameState.Id, user.Id.ToString(), after.VoiceChannel != null);
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