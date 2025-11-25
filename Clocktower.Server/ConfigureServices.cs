using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json.Serialization;
using Clocktower.Server.Common.Api.Auth;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Discord.Auth.Services;
using Clocktower.Server.Discord.Services;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Roles.Services;
using Clocktower.Server.Socket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace Clocktower.Server;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    extension(WebApplicationBuilder builder)
    {
        public void AddServices()
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
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly);

            builder.Services.Configure<Secrets>(builder.Configuration.GetSection(nameof(Secrets)));

            builder.Services.AddSingleton<IJwtWriter, JwtWriter>();
            builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
            builder.Services.AddSingleton<IGameStateStore, GameStateStore>();
            builder.Services.AddSingleton<ITownOccupancyStore, TownOccupancyStore>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
            builder.Services.AddSingleton<IDiscordBot, DiscordBot>();
            builder.Services.AddSingleton<IDiscordService, DiscordService>();
            builder.Services.AddSingleton<IDiscordTownService, DiscordTownService>();
            builder.Services.AddSingleton<IFileSystem, FileSystem>();

            builder.Services.AddScoped<IDiscordAuthService, DiscordAuthService>();
            builder.Services.AddScoped<IGameStateService, GameStateService>();
            builder.Services.AddScoped<IRolesService, RolesService>();
            builder.Services.AddScoped<IGameAuthorizationService, GameAuthorizationService>();
            builder.Services.AddScoped<IAuthorizationHandler, StoryTellerForGameHandler>();

            builder.Services.AddHostedService(provider => provider.GetRequiredService<IDiscordBot>());
        }

        private void ConfigureJson()
        {
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.Converters.Add(new ULongToStringConverter());
            });
            builder.Services.Configure<JsonOptions>(options => { options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
        }

        private void AddSwagger()
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));
                options.InferSecuritySchemes();
            });

            builder.Services.AddSwaggerGen(c => { c.SchemaFilter<EnumSchemaFilter>(); });
        }

        private void AddSignalR()
        {
            builder.Services.AddSignalR()
                .AddJsonProtocol(options => { options.PayloadSerializerOptions.Converters.Add(new ULongToStringConverter()); });
        }

        private void AddSerilog()
        {
            builder.Host.UseSerilog((context, configuration) => { configuration.ReadFrom.Configuration(context.Configuration); });
        }
    }
}