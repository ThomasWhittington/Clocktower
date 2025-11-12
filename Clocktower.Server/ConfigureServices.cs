using System.Text.Json.Serialization;
using Clocktower.Server.Discord.Auth.Services;
using Clocktower.Server.Discord.Services;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Socket;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Clocktower.Server;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Secrets>().AddEnvironmentVariables().Build();
        builder.Configuration.AddConfiguration(config);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp",
                policy => policy.WithOrigins(
                        "http://localhost:5120",
                        "http://localhost:5173",
                        "http://37.27.37.160",
                        "https://amarantosclocktower.web.app",
                        "https://clocktower.glasmerio.uk"

                        )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        builder.AddSerilog();
        builder.AddSwagger();
        builder.AddSignalR();
        builder.ConfigureJson();
        builder.Services.Configure<Secrets>(builder.Configuration.GetSection(nameof(Secrets)));
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
        builder.Services.AddSingleton<IDiscordAuthService, DiscordAuthService>();
        builder.Services.AddSingleton<DiscordBotService>();
        builder.Services.AddHostedService(provider => provider.GetRequiredService<DiscordBotService>());
        builder.Services.AddSingleton<IDiscordService, DiscordService>();
        builder.Services.AddSingleton<IDiscordTownService, DiscordTownService>();
        builder.Services.AddSingleton<GameStateService>();
        builder.Services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly);
    }

    private static void ConfigureJson(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.Converters.Add(new ULongToStringConverter());
        });
        builder.Services.Configure<JsonOptions>(options => { options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
    }

    private static void AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));
            options.InferSecuritySchemes();
        });

        builder.Services.AddSwaggerGen(c => { c.SchemaFilter<EnumSchemaFilter>(); });
    }

    private static void AddSignalR(this WebApplicationBuilder builder)
    {
        builder.Services.AddSignalR()
            .AddJsonProtocol(options => { options.PayloadSerializerOptions.Converters.Add(new ULongToStringConverter()); });
    }

    private static void AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) => { configuration.ReadFrom.Configuration(context.Configuration); });
    }
}