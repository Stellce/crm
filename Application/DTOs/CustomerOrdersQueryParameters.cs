namespace Application.DTOs;

public record CustomerOrdersQueryParameters : QueryParameters
{
    public decimal? MinTotalAmount { get; init; }
    public decimal? MaxTotalAmount { get; init; }
    public DateTimeOffset? CreatedFrom { get; init; }
    public DateTimeOffset? CreatedTo { get; init; }
}