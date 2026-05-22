namespace Application.Exceptions;

public sealed record ErrorInfo(
    string PublicCode,
    string Message
);