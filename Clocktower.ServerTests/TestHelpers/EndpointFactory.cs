using Microsoft.Extensions.DependencyInjection;

namespace Clocktower.ServerTests.TestHelpers;

public static class EndpointFactory
{
    public static TestEndpointRouteBuilder CreateBuilder()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRouting();

        var serviceProvider = services.BuildServiceProvider();
        return new TestEndpointRouteBuilder(serviceProvider);
    }
}