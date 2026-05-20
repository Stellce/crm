namespace Api.Dtos;

public record CustomerQueryParameters : QueryParameters
{

    public string? Search { get; init; }

    public override string SortBy { get; init; } = "Name";
}