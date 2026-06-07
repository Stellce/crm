namespace Application.DTOs;

public record CustomerTimelineReportQueryParameters : QueryParameters
{
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
}