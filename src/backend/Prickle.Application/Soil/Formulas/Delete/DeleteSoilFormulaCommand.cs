namespace Prickle.Application.Soil.Formulas.Delete;

public sealed record DeleteSoilFormulaCommand(Guid Id) : ICommand<Result>;