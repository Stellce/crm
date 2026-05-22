namespace Application.DTOs;

public abstract record QueryParameters
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public virtual string SortBy { get; init; } = "CreatedAt";
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
    public bool IsDescending => SortDirection == SortDirection.Desc;
    public int Skip => (Page - 1) * PageSize;
}