using System.Diagnostics.CodeAnalysis;
using Clocktower.Server.Data.Wrappers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using DiscordUser = Clocktower.Server.Data.Wrappers.DiscordUser;

namespace Clocktower.Server.Common.Services;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordBot(
    IOptions<Secrets> secretsOptions,
    IDiscordBotHandler botHandler
)
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

        _client.UserVoiceStateUpdated += async (user, before, after) =>
            await botHandler.HandleUserVoiceStateUpdate(
                new DiscordUser(user),
                new DiscordVoiceState(before),
                new DiscordVoiceState(after)
            );


        string token = _secrets.DiscordBotToken;
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public IDiscordGuild? GetGuild(string guildId)
    {
        if (!ulong.TryParse(guildId, out var id)) return null;
        var guild = _client.GetGuild(id);
        return guild != null ? new DiscordGuild(guild) : null;
    }

    public IEnumerable<IDiscordGuild> GetGuilds()
    {
        return _client.Guilds.Select(g => new DiscordGuild(g)).ToArray<IDiscordGuild>();
    }

    public async Task<IDiscordUser?> GetUserAsync(string userId)
    {
        if (!ulong.TryParse(userId, out var id)) return null;
        var user = await _client.GetUserAsync(id);
        return user != null ? new DiscordUser(user) : null;
    }

    public IDiscordUser? GetUser(string userId)
    {
        if (!ulong.TryParse(userId, out var id)) return null;
        var user = _client.GetUser(id);
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