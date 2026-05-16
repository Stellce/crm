namespace Api.Exceptions;

public class AppException : Exception
{
    public ErrorCode ErrorCode { get; }

    public AppException(ErrorCode errorCode)
        : base(errorCode.ToErrorInfo().Message)
    {
        ErrorCode = errorCode;
    }

    public AppException(ErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }


    public AppException(ErrorCode errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}