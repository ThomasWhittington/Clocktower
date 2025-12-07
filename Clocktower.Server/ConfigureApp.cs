using System.Diagnostics.CodeAnalysis;
using Clocktower.Server.Socket;
using Microsoft.Extensions.FileProviders;
using Serilog;

namespace Clocktower.Server;

[ExcludeFromCodeCoverage]
public static class ConfigureApp
{
    public static void Configure(this WebApplication app)
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, ".well-known")),
            RequestPath = "/.well-known"
        });
        app.UseCors("AllowReactApp");
        app.UseSerilogRequestLogging();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapEndpoints();

        app.MapHub<DiscordNotificationHub>("/serverHub");
    }
}