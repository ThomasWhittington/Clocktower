using Clocktower.Server.Common.Api.Filters;

namespace Clocktower.Server.Common.Api.Extensions;

public static class RouteHandlerBuilderExtensions
{
    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder SetOpenApiOperationId<T>() where T : class, IEndpoint
        {
            return builder.WithOpenApi(operation =>
            {
                operation.OperationId = typeof(T).Name.ToCamelCase() + "Api";
                return operation;
            });
        }
        
        public RouteHandlerBuilder WithRequestValidation<TRequest>()
        {
            return builder
                .AddEndpointFilter<RequestValidationFilter<TRequest>>()
                .ProducesValidationProblem();
        }
    }
}