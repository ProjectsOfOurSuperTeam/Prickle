namespace Prickle.Application.Soil.Formulas.Add;

public sealed record AddSoilFormulaCommand(string Name, IEnumerable<SoilFormulaItemDTO> FormulaItemDTOs) : ICommand<Result<SoilFormulaResponse>>;