namespace Prickle.Application.Soil.Formulas.Get;

public sealed record GetSoilFormulaByIdQuery(Guid Id) : IQuery<Result<SoilFormulaResponse>>;