namespace Clocktower.Server.Data.Extensions;

public static class ResultHttpExtensions
{
    extension(Result<string> result)
    {
        public Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>> ToHttpResult()
        {
            if (result.IsSuccess) return TypedResults.Ok(result.Value ?? string.Empty);

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