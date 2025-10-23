using Clocktower.Server.Common.Api.Filters;
using Clocktower.Server.WeatherForecast.Endpoints;

namespace Clocktower.Server;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("api")
            .AddEndpointFilter<RequestLoggingFilter>();
        
        endpoints.MapWeatherForecastEndpoints();
    }

    private static void MapWeatherForecastEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/weatherforecast")
            .WithTags("WeatherForecast");
            
        endpoints.MapPublicGroup()
            .MapEndpoint<GetWeatherForecast>();
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