namespace Application.DTOs;

public record OrderQueryParameters : QueryParameters
{
    public override string SortBy { get; init; } = "CreatedAt";

    public int? CustomerId { get; init; }
    public decimal? MinTotalAmount { get; init; }
    public decimal? MaxTotalAmount { get; init; }
    public DateTimeOffset? CreatedFrom { get; init; }
    public DateTimeOffset? CreatedTo { get; init; }
}