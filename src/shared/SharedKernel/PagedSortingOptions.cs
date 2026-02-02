namespace SharedKernel;

public record PagedSortingOptions
{
    public string? SortField { get; init; }
    public SortOrder? SortOrder { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public enum SortOrder
{
    Unsorted,
    Ascending,
    Descending
}
