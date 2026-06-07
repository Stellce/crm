namespace Application.DTOs;

public record CustomerTimelineEvent
{
    public DateTimeOffset EventDate { get; init; }  
    public string EventType { get; init; } = string.Empty;  
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }

    public int? OrderId { get; init; }
    public decimal? Amount { get; init; }

    public int? AttachmentId { get; init; }
    public string? FileName { get; init; }
}