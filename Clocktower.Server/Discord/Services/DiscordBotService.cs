using Discord;
using Discord.WebSocket;

namespace Clocktower.Server.Discord.Services;

public class DiscordBotService(Secrets secrets, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly DiscordSocketClient _client = new(new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.All
    });

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public DiscordSocketClient Client => _client;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;

        _client.UserVoiceStateUpdated += ClientOnUserVoiceStateUpdated;

        string token = secrets.DiscordBotToken;
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private Task ClientOnUserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
    {
        throw new NotImplementedException();
    }


    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        Console.WriteLine($"{_client.CurrentUser} is connected!");
        return Task.CompletedTask;
    }
}