using System.Diagnostics.CodeAnalysis;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Socket;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using DiscordUser = Clocktower.Server.Data.Wrappers.DiscordUser;

namespace Clocktower.Server.Common.Services;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordBot(IOptions<Secrets> secretsOptions, IGameStateStore gameStateStore, IServiceProvider serviceProvider, INotificationService notificationService)
    : BackgroundService, IDiscordBot
{
    private readonly Secrets _secrets = secretsOptions.Value;

    private readonly DiscordSocketClient _client = new(new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.All
    });

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;

        _client.UserVoiceStateUpdated += ClientOnUserVoiceStateUpdated;

        string token = _secrets.DiscordBotToken;
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ClientOnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
    {
        await HandleUserVoiceStateUpdate(new DiscordUser(user), new DiscordVoiceState(before), new DiscordVoiceState(after));
    }

    internal async Task HandleUserVoiceStateUpdate(IDiscordUser user, IDiscordVoiceState before, IDiscordVoiceState after)
    {
        var guildId = after.VoiceChannel?.Guild.Id ?? before.VoiceChannel?.Guild.Id;
        var channelsAreSame = before.VoiceChannel?.Id == after.VoiceChannel?.Id;
        if (!guildId.HasValue || channelsAreSame) return;

        var gameState = gameStateStore.GetGuildGames(guildId.Value).FirstOrDefault();
        if (gameState is null) return;

        using var scope = serviceProvider.CreateScope();
        var townService = scope.ServiceProvider.GetRequiredService<IDiscordTownService>();
        var (success, thisTownOccupancy, _) = await townService.GetTownOccupancy(guildId.Value);
        if (!success || thisTownOccupancy is null) return;
        thisTownOccupancy!.MoveUser(user, after.VoiceChannel);
        await notificationService.BroadcastTownOccupancyUpdate(gameState.Id, thisTownOccupancy);
        await notificationService.BroadcastUserVoiceStateChanged(gameState.Id, user.Id.ToString(), after.VoiceChannel != null);
    }

    public IDiscordGuild? GetGuild(ulong guildId)
    {
        var guild = _client.GetGuild(guildId);
        return guild != null ? new DiscordGuild(guild) : null;
    }

    public IEnumerable<IDiscordGuild> GetGuilds()
    {
        return _client.Guilds.Select(g => new DiscordGuild(g)).ToArray<IDiscordGuild>();
    }

    public async Task<IDiscordUser?> GetUserAsync(ulong userId)
    {
        var user = await _client.GetUserAsync(userId);
        return user != null ? new DiscordUser(user) : null;
    }

    public IDiscordUser? GetUser(ulong userId)
    {
        var user = _client.GetUser(userId);
        return user != null ? new DiscordUser(user) : null;
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