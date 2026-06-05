namespace Application.DTOs;

public record OrderAttachmentDownload(
    Stream Content,
    string OriginalFileName,
    string ContentType,
    long SizeBytes
);