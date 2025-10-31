using System.Text.Json.Serialization;
using Clocktower.Server.Common;
using Clocktower.Server.Discord.Services;
using Microsoft.AspNetCore.Http.Json;
using Serilog;

namespace Clocktower.Server;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Secrets>().Build();
        var secrets = config.GetSection(nameof(Secrets)).Get<Secrets>()!;


        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp",
                policy => policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        builder.AddSerilog();
        builder.AddSwagger();
        builder.AddSignalR();
        builder.ConfigureJson();
        builder.Services.AddSingleton(secrets);
        builder.Services.AddSingleton<DiscordBotService>();
        builder.Services.AddSingleton<DiscordService>();
        builder.Services.AddSingleton<GameStateService>();
        builder.Services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly);
    }

    private static void ConfigureJson(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpJsonOptions(options => { options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
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
        builder.Services.AddSignalR();
    }

    private static void AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) => { configuration.ReadFrom.Configuration(context.Configuration); });
    }
}