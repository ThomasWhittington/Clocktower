using System.Reflection;
using Clocktower.Server.Common.Api.Filters;

namespace Clocktower.Server;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("api")
            .AddEndpointFilter<RequestLoggingFilter>();

        endpoints.MapRolesEndpoints();
        endpoints.MapGamesEndpoints();
        endpoints.MapDiscordEndpoints();
        endpoints.MapDiscordAuthEndpoints();
        endpoints.MapDiscordTownEndpoints();
    }

    private static void MapRolesEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/roles")
            .WithTags("Roles");

        endpoints.MapPublicGroup()
            .MapEndpointsFromNamespace("Clocktower.Server.Roles.Endpoints");
    }

    private static void MapGamesEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/games")
            .WithTags("Games");

        endpoints.MapPublicGroup()
            .MapEndpointsFromNamespace("Clocktower.Server.Game.Endpoints");
    }

    private static void MapDiscordEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/discord")
            .WithTags("Discord");

        endpoints.MapPublicGroup()
            .MapEndpointsFromNamespace("Clocktower.Server.Discord.Endpoints");
    }
    
    private static void MapDiscordAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/discord/auth")
            .WithTags("Discord Auth");

        endpoints.MapPublicGroup()
            .MapEndpointsFromNamespace("Clocktower.Server.Discord.Auth.Endpoints");
    }
    
    private static void MapDiscordTownEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/discord/town")
            .WithTags("Discord Town");

        endpoints.MapPublicGroup()
            .MapEndpointsFromNamespace("Clocktower.Server.Discord.Town.Endpoints");
    }

    private static RouteGroupBuilder MapPublicGroup(this IEndpointRouteBuilder app, string? prefix = null)
    {
        return app.MapGroup(prefix ?? string.Empty)
            .AllowAnonymous();
    }

    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app) where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }

    public static IEndpointRouteBuilder MapEndpointsFromNamespace(this IEndpointRouteBuilder builder, string namespaceName)
    {
        var endpointTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.Namespace == namespaceName &&
                        t is { IsClass: true, IsAbstract: false } &&
                        typeof(IEndpoint).IsAssignableFrom(t));

        var mapEndpointMethod = typeof(Endpoints)
            .GetMethods()
            .First(m => m is { Name: nameof(MapEndpoint), IsGenericMethodDefinition: true });

        foreach (var endpointType in endpointTypes)
        {
            var genericMethod = mapEndpointMethod.MakeGenericMethod(endpointType);
            genericMethod.Invoke(null, [builder]);
        }

        return builder;
    }
}