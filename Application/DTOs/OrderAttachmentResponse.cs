namespace Application.DTOs;

public record OrderAttachmentResponse(
    int Id,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset CreatedAt
);