namespace Application.Exceptions;

public enum ErrorCode
{
    CustomerNotFound,
    CustomerAlreadyExists,

    OrderNotFound,

    UserAlreadyExists,
    UserNotFound,

    InvalidCredentials,
    InvalidAccessToken,

    Forbidden,

    InternalServerError
}