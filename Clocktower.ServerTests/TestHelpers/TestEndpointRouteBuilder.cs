using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Clocktower.ServerTests.TestHelpers;

public class TestEndpointRouteBuilder(IServiceProvider serviceProvider) : IEndpointRouteBuilder
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public ICollection<EndpointDataSource> DataSources { get; } = new List<EndpointDataSource>();

    public IApplicationBuilder CreateApplicationBuilder() => new ApplicationBuilder(ServiceProvider);
}