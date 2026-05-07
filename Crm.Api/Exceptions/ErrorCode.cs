namespace Crm.Api.Exceptions;

public enum ErrorCode
{
    CustomerNotFound,
    CustomerAlreadyExists,

    OrderNotFound,

    UserNotFound,

    InvalidCredentials,

    InternalServerError
}