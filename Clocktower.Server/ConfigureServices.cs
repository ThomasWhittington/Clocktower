using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Serilog;

namespace Clocktower.Server;

public static class ConfigureServices
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.AddSerilog();
        builder.AddSwagger();
        builder.ConfigureJson();
        builder.Services.AddSingleton<GameStateService>();
        builder.Services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly);
    }

    private static void ConfigureJson(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpJsonOptions(options => { options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
        builder.Services.Configure<JsonOptions>(options => { options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
    }

    private static void AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));
            options.InferSecuritySchemes();
        });

        builder.Services.AddSwaggerGen(c => { c.SchemaFilter<EnumSchemaFilter>(); });
    }

    private static void AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) => { configuration.ReadFrom.Configuration(context.Configuration); });
    }
}