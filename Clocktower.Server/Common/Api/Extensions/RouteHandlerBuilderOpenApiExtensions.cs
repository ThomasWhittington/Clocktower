namespace Clocktower.Server.Common.Api.Extensions;

public static class RouteHandlerBuilderOpenApiExtensions
{
    public static RouteHandlerBuilder SetOpenApiOperationId(this RouteHandlerBuilder builder, string operationId)
    {
        return builder.WithOpenApi(operation =>
        {
            operation.OperationId = operationId.ToCamelCase() + "Api";
            return operation;
        });
    }

    public static RouteHandlerBuilder SetOpenApiOperationId<T>(this RouteHandlerBuilder builder) where T : class, IEndpoint
    {
        return builder.WithOpenApi(operation =>
        {
            operation.OperationId = typeof(T).Name.ToCamelCase() + "Api";
            return operation;
        });
    }
}