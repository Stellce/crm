namespace Application.DTOs;

public record CustomerSalesReportQueryParameters : QueryParameters
{
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
    public decimal MinRevenue { get; init; }

    public override string SortBy { get; init; } = "TotalRevenue";
};