using Application.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Api.Exceptions;

public static class ErrorCodeExtensions
{
    public static int ToStatusCode(this ErrorCode errorCode)
    {
        return errorCode switch
        {
            ErrorCode.CustomerNotFound => StatusCodes.Status404NotFound,
            ErrorCode.CustomerAlreadyExists => StatusCodes.Status409Conflict,

            ErrorCode.OrderNotFound => StatusCodes.Status404NotFound,

            ErrorCode.UserAlreadyExists => StatusCodes.Status409Conflict,
            ErrorCode.UserNotFound => StatusCodes.Status404NotFound,

            ErrorCode.AttachmentNotFound => StatusCodes.Status404NotFound,

            ErrorCode.MaxFileSizeExceeded => StatusCodes.Status409Conflict,
            ErrorCode.InvalidFileType => StatusCodes.Status409Conflict,
            ErrorCode.FileNotFound => StatusCodes.Status404NotFound,
            ErrorCode.FileIsEmpty => StatusCodes.Status400BadRequest,

            ErrorCode.InvalidDateRange => StatusCodes.Status400BadRequest,

            ErrorCode.InvalidCredentials => StatusCodes.Status401Unauthorized,
            ErrorCode.InvalidAccessToken => StatusCodes.Status401Unauthorized,
            ErrorCode.InvalidResetToken => StatusCodes.Status400BadRequest,
            ErrorCode.Forbidden => StatusCodes.Status403Forbidden,

            _ => StatusCodes.Status500InternalServerError
        };
    }
}