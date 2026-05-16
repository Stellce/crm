namespace Api.Exceptions;

public enum ErrorCode
{
    CustomerNotFound,
    CustomerAlreadyExists,

    OrderNotFound,

    UserAlreadyExists,
    UserNotFound,

    Unauthorized,

    Forbidden,

    InternalServerError
}