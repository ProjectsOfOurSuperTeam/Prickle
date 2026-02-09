namespace Prickle.Application.Plants.GetAll;

public sealed record GetAllPlantsQuery(string? Name) : PagedSortingOptions, IQuery<Result<PlantsResponse>>;