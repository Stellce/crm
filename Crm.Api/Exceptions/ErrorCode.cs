namespace Crm.Api.Exceptions;

public enum ErrorCode
{
    CustomerNotFound,
    CustomerAlreadyExists,

    OrderNotFound,

    UserNotFound,

    Unauthorized,

    Forbidden,

    InternalServerError
}