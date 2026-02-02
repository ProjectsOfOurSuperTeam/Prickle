using Prickle.Application.Soil.Types;

namespace Prickle.Application.Soil.Formulas;

public sealed record SoilFormulaItemResponse(SoilTypeResponse SoilType, int Percentage, int Order);