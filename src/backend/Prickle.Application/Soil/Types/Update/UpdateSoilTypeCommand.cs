namespace Prickle.Application.Soil.Types.Update;

public sealed record UpdateSoilTypeCommand(int Id, string NewName) : ICommand<Result<SoilTypeResponse>>;
