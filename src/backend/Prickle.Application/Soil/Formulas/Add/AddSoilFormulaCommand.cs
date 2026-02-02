namespace Prickle.Application.Soil.Formulas.Add;

public sealed record AddSoilFormulaCommand(string Name, IEnumerable<AddSoilFormulaItemDTO> FormulaItemDTOs) : ICommand<Result<SoilFormulaResponse>>;