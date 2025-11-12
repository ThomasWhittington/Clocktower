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
using Clocktower.Server;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServices();
    var app = builder.Build();
    app.UseHttpsRedirection();
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