using System.ComponentModel;

namespace SharedKernel;

public record PagedRequest
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    [DefaultValue(DefaultPage)]
    public int Page { get; init; } = DefaultPage;

    [DefaultValue(DefaultPageSize)]
    public int PageSize { get; init; } = DefaultPageSize;
}
