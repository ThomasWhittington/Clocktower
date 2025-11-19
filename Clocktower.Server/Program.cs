global using Microsoft.AspNetCore.Http.HttpResults;
global using Clocktower.Server.Common.Api;
global using FluentValidation;
global using JetBrains.Annotations;
global using Clocktower.Server.Data;
global using Clocktower.Server.Data.Types.Enum;
global using Clocktower.Server.Data.Types;
global using Clocktower.Server.Data.Types.Role;
global using Clocktower.Server.Data.Filters;
global using Clocktower.Server.Game.Services;
global using Clocktower.Server.Common.Api.Extensions;
global using Clocktower.Server.Common;
global using Clocktower.Server.Data.Stores;
global using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Text;
using Clocktower.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;

[assembly: InternalsVisibleTo("Clocktower.ServerTests")]

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);


    builder.AddServices();

    var secrets = builder.Configuration.GetSection(nameof(Secrets)).Get<Secrets>()!;
    var secretsValidation = secrets.HasAllSecrets();
    if (!secretsValidation.success)
    {
        throw new InvalidConfigurationException($"Secrets invalid: {secretsValidation.message}");
    }

    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = secrets.ServerUri,
                ValidateAudience = true,
                ValidAudience = secrets.Jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secrets.Jwt.SigningKey)
                ),
                ValidateLifetime = true,
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/discordHub"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("StoryTellerForGame", policy =>
            {
                policy.RequireClaim("is_storyteller", "true");
                policy.AddRequirements(new StoryTellerForGameRequirement());
            }
        );
    var app = builder.Build();
    app.Configure();


    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}