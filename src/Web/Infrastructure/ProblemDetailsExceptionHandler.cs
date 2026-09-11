using Microsoft.AspNetCore.Diagnostics;
using ForbiddenAccessException = WareStockApi.Application.Common.Exceptions.ForbiddenAccessException;
using NotFoundException = WareStockApi.Application.Common.Exceptions.NotFoundException;
using ValidationException = WareStockApi.Application.Common.Exceptions.ValidationException;

namespace WareStockApi.Web.Infrastructure;

/// <summary>
/// Converts well-known application exceptions into the shared <see cref="ApiErrorResponse"/>
/// envelope, mapping <see cref="ValidationException"/> → 400, <see cref="UnauthorizedAccessException"/> → 401,
/// <see cref="ForbiddenAccessException"/> → 403, and <see cref="NotFoundException"/> → 404.
/// Unrecognised exceptions are not handled and fall through to the default middleware.
/// </summary>
public class ProblemDetailsExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, errorCode, errors) = exception switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest, "VALIDATION_ERROR", ve.Errors),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "UNAUTHORIZED", EmptyErrors),
            ForbiddenAccessException => (StatusCodes.Status403Forbidden, "FORBIDDEN", EmptyErrors),
            NotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND", EmptyErrors),
            _ => (-1, "", EmptyErrors)
        };

        if (statusCode == -1) return false;

        var response = new ApiErrorResponse
        {
            ErrorCode = errorCode,
            Errors = errors,
            Message = exception.Message,
            RequestId = Guid.NewGuid().ToString(),
            StatusCode = statusCode
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }

    private static readonly IDictionary<string, string[]> EmptyErrors = new Dictionary<string, string[]>();
}
