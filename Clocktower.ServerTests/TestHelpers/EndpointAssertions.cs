using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;

namespace Clocktower.ServerTests.TestHelpers;

public static class EndpointAssertions
{
    extension(IEndpointRouteBuilder builder)
    {
        public RouteEndpoint GetEndpoint([StringSyntax("Route"), RouteTemplate] string routePattern)
        {
            var endpoint = builder.DataSources
                .SelectMany(ds => ds.Endpoints)
                .OfType<RouteEndpoint>()
                .FirstOrDefault(e => e.RoutePattern.RawText == routePattern);

            endpoint.Should().NotBeNull($"Endpoint '{routePattern}' should be registered");
            return endpoint!;
        }
    }

    extension(RouteEndpoint endpoint)
    {
        public RouteEndpoint ShouldHaveMethod(HttpMethod method)
        {
            endpoint.GetMetadata<HttpMethodMetadata>().HttpMethods.Should().Contain(method.ToString());
            return endpoint;
        }

        public RouteEndpoint ShouldHaveSummaryAndDescription(string summaryDescription)
        {
            ShouldHaveSummary(endpoint, summaryDescription);
            ShouldHaveDescription(endpoint, summaryDescription);
            return endpoint;
        }

        public RouteEndpoint ShouldHaveSummary(string summary)
        {
            endpoint.GetMetadata<IEndpointSummaryMetadata>().Summary.Should().Be(summary);
            return endpoint;
        }

        public RouteEndpoint ShouldHaveDescription(string description)
        {
            endpoint.GetMetadata<IEndpointDescriptionMetadata>().Description.Should().Be(description);
            return endpoint;
        }

        public RouteEndpoint ShouldHaveOperationId(string operationId)
        {
            endpoint.GetMetadata<OpenApiOperation>().OperationId.Should().Be(operationId);
            return endpoint;
        }

        public RouteEndpoint ShouldAllowAnonymous()
        {
            endpoint.GetMetadata<IAllowAnonymous>().Should().NotBeNull();
            return endpoint;
        }

        public RouteEndpoint ShouldHaveStorytellerAuthorization()
        {
            endpoint.GetMetadata<AuthorizeAttribute>().Policy.Should().Be("StoryTellerForGame");
            return endpoint;
        }

        public RouteEndpoint ShouldHaveValidation()
        {
            var producesMetadata = endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
                .Where(o => o.StatusCode == 400);
            producesMetadata.Should()
                .Contain(o => o.Type == typeof(HttpValidationProblemDetails), "Endpoints with validation should document a 400 Bad Request response with return of HttpValidationProblemDetails");
            return endpoint;
        }

        public T GetMetadata<T>() where T : class
        {
            var metaData = endpoint.Metadata.GetMetadata<T>();
            metaData.Should().NotBeNull();
            return metaData;
        }
    }
}