using Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails;

        if (exception is AppException appException)
        {
            var error = appException.ErrorCode.ToErrorInfo();
            var statusCode = appException.ErrorCode.ToStatusCode();

            problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = error.PublicCode,
                Detail = error.Message,
                Type = $"https://httpstatuses.com/{statusCode}"
            };
        }
        else
        {
            logger.LogError(exception, "Unhandled exception");

            problemDetails = new ProblemDetails
            {
                Status = 500,
                Title = "INTERNAL_SERVER_ERROR",
                Detail = "Unexpected server error",
                Type = "https://httpstatuses.com/500"
            };
        }

        context.Response.StatusCode = problemDetails.Status.Value;

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}