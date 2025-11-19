using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
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
        public void ShouldHaveMethod(HttpMethod method)
        {
            var metadata = endpoint.Metadata.GetMetadata<HttpMethodMetadata>();
            metadata.Should().NotBeNull("HTTP Method metadata is missing");
            metadata!.HttpMethods.Should().Contain(method.ToString());
        }

        public void ShouldHaveSummaryAndDescription(string summaryDescription)
        {
            ShouldHaveSummary(endpoint, summaryDescription);
            ShouldHaveDescription(endpoint, summaryDescription);
        }

        public void ShouldHaveSummary(string summary)
        {
            endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>()
                ?.Summary.Should().Be(summary);
        }

        public void ShouldHaveDescription(string description)
        {
            endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()
                ?.Description.Should().Be(description);
        }

        public void ShouldHaveEndpointName(string name)
        {
            endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()
                ?.EndpointName.Should().Be(name);
        }

        public void ShouldHaveOperationId(string operationId)
        {
            endpoint.Metadata.GetMetadata<OpenApiOperation>()
                ?.OperationId.Should().Be(operationId);
        }

        public void ShouldHaveValidation()
        {
            var producesMetadata = endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
                .Where(o => o.StatusCode == 400);
            producesMetadata.Should()
                .Contain(o => o.Type == typeof(HttpValidationProblemDetails), "Endpoints with validation should document a 400 Bad Request response with return of HttpValidationProblemDetails");
        }
    }
}