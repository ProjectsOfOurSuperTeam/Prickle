namespace Prickle.Application.Soil.Types.Delete;

public record DeleteSoilTypeCommand(int Id) : ICommand<Result>;
