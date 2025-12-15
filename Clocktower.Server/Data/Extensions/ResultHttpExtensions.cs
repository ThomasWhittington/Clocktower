namespace Clocktower.Server.Data.Extensions;

public static class ResultHttpExtensions
{
    extension<T>(Result<T> result)
    {
        public Results<Ok<T>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>> ToHttpResult()
        {
            if (result is { IsSuccess: true }) return TypedResults.Ok(result.Value);

            var error = result.Error!;
            var errorResponse = new ErrorResponse(error.Code, error.Message);
            return error.Kind switch
            {
                ErrorKind.NotFound => TypedResults.NotFound(errorResponse),
                _ => TypedResults.BadRequest(errorResponse)
            };
        }
    }
}