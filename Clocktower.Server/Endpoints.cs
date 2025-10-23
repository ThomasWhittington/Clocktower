using Clocktower.Server.Common.Api.Filters;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Players.Endpoints;
using Clocktower.Server.Roles.Endpoints;
using Clocktower.Server.WeatherForecast.Endpoints;

namespace Clocktower.Server;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("api")
            .AddEndpointFilter<RequestLoggingFilter>();

        endpoints.MapWeatherForecastEndpoints();
        endpoints.MapPlayersEndpoints();
        endpoints.MapRolesEndpoints();
        endpoints.MapGamesEndpoints();
    }

    private static void MapWeatherForecastEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/weatherforecast")
            .WithTags("WeatherForecast");

        endpoints.MapPublicGroup()
            .MapEndpoint<GetWeatherForecast>();
    }

    private static void MapPlayersEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/players")
            .WithTags("Players");

        endpoints.MapPublicGroup()
            .MapEndpoint<AddPlayer>();
    }

    private static void MapRolesEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/roles")
            .WithTags("Roles");

        endpoints.MapPublicGroup()
            .MapEndpoint<GetRoles>()
            ;
    }

    private static void MapGamesEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/games")
            .WithTags("Games");

        endpoints.MapPublicGroup()
            .MapEndpoint<GetGames>()
            .MapEndpoint<GetGame>()
            .MapEndpoint<StartGame>()
            ;
    }

    private static RouteGroupBuilder MapPublicGroup(this IEndpointRouteBuilder app, string? prefix = null)
    {
        return app.MapGroup(prefix ?? string.Empty)
            .AllowAnonymous();
    }

    private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app) where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}