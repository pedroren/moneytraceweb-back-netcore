using ErrorOr;

namespace MoneyTrace.RestBackend.Dto;

public static class ErrorOrExtensions
{
    /// <summary>
    /// Converts a list of errors from OrError to a Minimal APIs TypedResults
    /// </summary>
    /// <param name="errors">Error list</param>
    /// <returns>TypedResults.ValidationProblem or TypedResults.Problem</returns>
    public static IResult ToTypedResultsError(this List<Error> errors)
    {
        //If all the errors are validation errors, return a validation problem
        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            Dictionary<string, string[]> validationErrors = errors
            .GroupBy(error => error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Description).ToArray());
            return TypedResults.ValidationProblem(validationErrors);
        }
        //Other kinds of error, return the first one
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