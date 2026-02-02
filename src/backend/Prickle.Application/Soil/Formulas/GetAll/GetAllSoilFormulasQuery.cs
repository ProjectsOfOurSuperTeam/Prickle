namespace Prickle.Application.Soil.Formulas.GetAll;

public sealed record GetAllSoilFormulasQuery(string? Name, IEnumerable<int>? SoilTypeIds) : PagedSortingOptions, IQuery<Result<SoilFormulasResponse>>;
