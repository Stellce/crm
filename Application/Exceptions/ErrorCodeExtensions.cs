namespace Application.Exceptions;

public static class ErrorCodeExtensions
{
    public static ErrorInfo ToErrorInfo(this ErrorCode code)
    {
        return code switch
        {
            ErrorCode.CustomerNotFound => new ErrorInfo(
                "CUSTOMER_NOT_FOUND",
                "Customer not found"
            ),

            ErrorCode.CustomerAlreadyExists => new ErrorInfo(
                "CUSTOMER_ALREADY_EXISTS",
                "Customer already exists"
            ),

            ErrorCode.OrderNotFound => new ErrorInfo(
                "ORDER_NOT_FOUND",
                "Order not found"
            ),

            ErrorCode.UserAlreadyExists => new ErrorInfo(
                "USER_ALREADY_EXISTS",
                "User already exists"
            ),

            ErrorCode.UserNotFound => new ErrorInfo(
                "USER_NOT_FOUND",
                "User not found"
            ),

            ErrorCode.InvalidCredentials => new ErrorInfo(
                "INVALID_CREDENTIALS",
                "Invalid credentials"
            ),

            ErrorCode.InvalidAccessToken => new ErrorInfo(
                "INVALID_ACCESS_TOKEN",
                "Invalid access token"
            ),

            ErrorCode.InvalidResetToken => new ErrorInfo(
                "INVALID_RESET_TOKEN",
                "Invalid reset token"
            ),

            ErrorCode.Forbidden => new ErrorInfo(
                "FORBIDDEN",
                "User does not have permission to perform this action"
            ),

            _ => new ErrorInfo(
                "INTERNAL_SERVER_ERROR",
                "Unexpected server error"
            )
        };
    }
}