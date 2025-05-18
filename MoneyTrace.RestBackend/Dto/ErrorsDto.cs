using ErrorOr;

namespace MoneyTrace.RestBackend.Dto;

public static class ErrorOrExtensions
{
    public static IResult ToTypedResultsError(this List<Error> errors)
    {
        var error = errors.FirstOrDefault(); //Only first error for now
        var statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };
        return TypedResults.Problem(statusCode: statusCode, title: error.Description);
    }
}