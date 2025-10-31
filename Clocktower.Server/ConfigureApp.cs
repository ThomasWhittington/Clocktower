using Serilog;

namespace Clocktower.Server;

public static class ConfigureApp
{
    public static void Configure(this WebApplication app)
    {
        app.UseCors("AllowReactApp");
        app.UseSerilogRequestLogging();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.MapEndpoints();
    }
}