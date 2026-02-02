namespace Prickle.Application.Soil.Formulas;

public sealed record SoilFormulaResponse(Guid Id, string Name, IEnumerable<SoilFormulaItemResponse> Items);