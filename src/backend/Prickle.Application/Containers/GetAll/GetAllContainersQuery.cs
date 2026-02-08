namespace Prickle.Application.Containers.GetAll;

public sealed record GetAllContainersQuery(string? Name) : PagedSortingOptions, IQuery<Result<ContainersResponse>>;