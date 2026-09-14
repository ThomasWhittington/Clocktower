using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json.Serialization;
using Clocktower.Server.Admin.Services;
using Clocktower.Server.Common.Api.Auth;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Discord;
using Clocktower.Server.Discord.Auth.Services;
using Clocktower.Server.Discord.GameAction.Services;
using Clocktower.Server.Discord.Services;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Roles.Services;
using Clocktower.Server.Socket;
using Clocktower.Server.Socket.Services;
using Clocktower.Server.Timer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Core;
using Serilog.Events;
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
            builder.Services.AddSignalR();
            builder.ConfigureJson();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly);

            builder.Services.Configure<Secrets>(builder.Configuration.GetSection(nameof(Secrets)));

            builder.Services.AddSingleton<IIdGenerator, IdGenerator>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<ITalkRequestManager, TalkRequestManager>();
            builder.Services.AddSingleton<IJwtWriter, JwtWriter>();
            builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
            builder.Services.AddSingleton<ITimerCoordinator, TimerCoordinator>();
            builder.Services.AddSingleton<IHubStateManager, HubStateManager>();
            builder.Services.AddSingleton<IGamePerspectiveStore, GamePerspectiveStore>();
            builder.Services.AddSingleton<IDiscordTownStore, DiscordTownStore>();
            builder.Services.AddSingleton<IUserIdentityStore, UserIdentityStore>();
            builder.Services.AddSingleton<IDiscordTownManager, DiscordTownManager>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IGameBroadcastService, GameBroadcastService>();
            builder.Services.AddSingleton<IDiscordBotHandler, DiscordBotHandler>();
            builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
            builder.Services.AddSingleton<IDiscordBot, DiscordBot>();
            builder.Services.AddSingleton<IDiscordService, DiscordService>();
            builder.Services.AddSingleton<IDiscordTownService, DiscordTownService>();
            builder.Services.AddSingleton<IFileSystem, FileSystem>();
            builder.Services.AddSingleton<IDiscordAuthApiService, DiscordAuthApiService>();
            builder.Services.AddSingleton<IDiscordConstantsService, DiscordConstantsService>();
            builder.Services.AddSingleton<IGamePerspectiveService, GamePerspectiveService>();
            builder.Services.AddSingleton<IVotingService, VotingService>();

            builder.Services.AddScoped<IScriptProvider, ScriptProvider>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IDiscordAuthService, DiscordAuthService>();
            builder.Services.AddScoped<IGameService, GameService>();
            builder.Services.AddScoped<IDiscordGameActionService, DiscordGameActionService>();
            builder.Services.AddScoped<IRolesService, RolesService>();
            builder.Services.AddScoped<IGameAuthorizationService, GameAuthorizationService>();
            builder.Services.AddScoped<IAuthorizationHandler, StoryTellerForGameHandler>();
            builder.Services.AddScoped<ITimerService, TimerService>();

            builder.Services.AddHostedService(provider => provider.GetRequiredService<IDiscordBot>());
            builder.Services.AddHostedService(provider => provider.GetRequiredService<IVotingService>());
        }

        private void ConfigureJson()
        {
            builder.Services.ConfigureHttpJsonOptions(options => { options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
            builder.Services.Configure<JsonOptions>(options => { options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
        }

        private void AddSwagger()
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));
                options.InferSecuritySchemes();
                options.SchemaFilter<EnumSchemaFilter>();

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        private void AddSerilog()
        {
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.With(new MachineNameEnricher())
                    .Enrich.With(new RenderedMessageEnricher())
                    .Enrich.With(new ActivityEnricher())
                    .Enrich.With(new HttpContextEnricher());
            }, preserveStaticLogger: false);
        }
    }
}

[ExcludeFromCodeCoverage]
public class RenderedMessageEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var rendered = logEvent.RenderMessage();
        var property = propertyFactory.CreateProperty("RenderedMessage", rendered);
        logEvent.AddPropertyIfAbsent(property);
    }
}

[ExcludeFromCodeCoverage]
public class ActivityEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity == null)
            return;

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString()));

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));
    }
}

[ExcludeFromCodeCoverage]
public class HttpContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = new HttpContextAccessor().HttpContext;
        if (httpContext == null)
            return;

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("RequestPath", httpContext.Request.Path.Value));

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("RequestId", httpContext.TraceIdentifier));

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("ConnectionId", httpContext.Connection.Id));
    }
}

[ExcludeFromCodeCoverage]
public class MachineNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var machine = Environment.MachineName;
        var property = propertyFactory.CreateProperty("MachineName", machine);
        logEvent.AddPropertyIfAbsent(property);
    }
}