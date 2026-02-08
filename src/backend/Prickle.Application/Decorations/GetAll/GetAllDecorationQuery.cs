namespace Prickle.Application.Decorations.GetAll;

public sealed record GetAllDecorationQuery(string? Name) : PagedSortingOptions, IQuery<Result<DecorationsResponse>>;