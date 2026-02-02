namespace Prickle.Application.Soil.Formulas.Update;

public sealed record UpdateSoilFormulaCommand(Guid Id, string NewName, IEnumerable<SoilFormulaItemDTO> FormulaItemDTOs) : ICommand<Result<SoilFormulaResponse>>;