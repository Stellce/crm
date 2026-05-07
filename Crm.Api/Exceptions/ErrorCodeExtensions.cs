namespace Crm.Api.Exceptions;

public static class ErrorCodeExtensions
{
    public static ErrorInfo ToErrorInfo(this ErrorCode code)
    {
        return code switch
        {
            ErrorCode.CustomerNotFound => new ErrorInfo(
                StatusCodes.Status404NotFound,
                "CUSTOMER_NOT_FOUND",
                "Customer not found"
            ),

            ErrorCode.CustomerAlreadyExists => new ErrorInfo(
                StatusCodes.Status409Conflict,
                "CUSTOMER_ALREADY_EXISTS",
                "Customer already exists"
            ),

            ErrorCode.OrderNotFound => new ErrorInfo(
                StatusCodes.Status404NotFound,
                "ORDER_NOT_FOUND",
                "Order not found"
            ),

            ErrorCode.InvalidCredentials => new ErrorInfo(
                StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED",
                "User not authorized"
            ),

            ErrorCode.UserNotFound => new ErrorInfo(
                StatusCodes.Status404NotFound,
                "USER_NOT_FOUND",
                "User not found"
            ),

            _ => new ErrorInfo(
                StatusCodes.Status500InternalServerError,
                "INTERNAL_SERVER_ERROR",
                "Unexpected server error"
            )
        };
    }
}