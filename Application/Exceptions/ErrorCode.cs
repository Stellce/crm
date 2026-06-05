namespace Application.Exceptions;

public enum ErrorCode
{
    CustomerNotFound,
    CustomerAlreadyExists,

    OrderNotFound,

    UserAlreadyExists,
    UserNotFound,

    AttachmentNotFound,

    MaxFileSizeExceeded,
    InvalidFileType,
    FileNotFound,
    FileIsEmpty,

    InvalidCredentials,
    InvalidAccessToken,
    InvalidResetToken,

    Forbidden,

    InternalServerError
}