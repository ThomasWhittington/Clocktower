using System.Reflection;
using Clocktower.Server.Common.Api.Filters;

namespace Clocktower.Server;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("api")
            .AddEndpointFilter<RequestLoggingFilter>();

        endpoints.MapAdminEndpoints();
        endpoints.MapRolesEndpoints();
        endpoints.MapGamesEndpoints();
        endpoints.MapDiscordEndpoints();
        endpoints.MapDiscordAuthEndpoints();
        endpoints.MapDiscordTownEndpoints();
    }

    extension(IEndpointRouteBuilder app)
    {
        private void MapAdminEndpoints()
        {
            var endpoints = app.MapGroup("/admin")
                .WithTags("Admin");

            endpoints.MapPublicGroup()
                .MapEndpointsFromNamespace("Clocktower.Server.Admin.Endpoints");
        }

        private void MapRolesEndpoints()
        {
            var endpoints = app.MapGroup("/roles")
                .WithTags("Roles");

            endpoints.MapPublicGroup()
                .MapEndpointsFromNamespace("Clocktower.Server.Roles.Endpoints");
        }

        private void MapGamesEndpoints()
        {
            var endpoints = app.MapGroup("/games")
                .WithTags("Games");

            endpoints.MapPublicGroup()
                .MapEndpointsFromNamespace("Clocktower.Server.Game.Endpoints");
        }

        private void MapDiscordEndpoints()
        {
            var endpoints = app.MapGroup("/discord")
                .WithTags("Discord");

            endpoints.MapPublicGroup()
                .MapEndpointsFromNamespace("Clocktower.Server.Discord.Endpoints");
        }

        private void MapDiscordAuthEndpoints()
        {
            var endpoints = app.MapGroup("/discord/auth")
                .WithTags("Discord Auth");

            endpoints.MapPublicGroup()
                .MapEndpointsFromNamespace("Clocktower.Server.Discord.Auth.Endpoints");
        }

        private void MapDiscordTownEndpoints()
        {
            var endpoints = app.MapGroup("/discord/town")
                .WithTags("Discord Town");

            endpoints.MapPublicGroup()
                .MapEndpointsFromNamespace("Clocktower.Server.Discord.Town.Endpoints");
        }

        private RouteGroupBuilder MapPublicGroup(string? prefix = null)
        {
            return app.MapGroup(prefix ?? string.Empty);
        }

        public IEndpointRouteBuilder MapEndpoint<TEndpoint>() where TEndpoint : IEndpoint
        {
            TEndpoint.Map(app);
            return app;
        }

        public IEndpointRouteBuilder MapEndpointsFromNamespace(string namespaceName)
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
                genericMethod.Invoke(null, [app]);
            }

            return app;
        }
    }
}