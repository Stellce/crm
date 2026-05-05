namespace Crm.Api.Exceptions;

public sealed record ErrorInfo(
    int StatusCode,
    string PublicCode,
    string Message
);