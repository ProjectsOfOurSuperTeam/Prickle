namespace Prickle.Application.Soil.Types.GetAll;

public sealed record GetAllSoilTypeQuery(string? Name) : PagedSortingOptions, IQuery<Result<SoilTypesResponse>>;