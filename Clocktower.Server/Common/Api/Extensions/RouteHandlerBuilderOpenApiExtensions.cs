namespace Clocktower.Server.Common.Api.Extensions;

public static class RouteHandlerBuilderOpenApiExtensions
{
    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder SetOpenApiOperationId(string operationId)
        {
            return builder.WithOpenApi(operation =>
            {
                operation.OperationId = operationId.ToCamelCase() + "Api";
                return operation;
            });
        }

        public RouteHandlerBuilder SetOpenApiOperationId<T>() where T : class, IEndpoint
        {
            return builder.WithOpenApi(operation =>
            {
                operation.OperationId = typeof(T).Name.ToCamelCase() + "Api";
                return operation;
            });
        }
    }
}