namespace Prickle.Application.Soil.Types.Add;

public sealed record AddSoilTypeCommand(string Name, string? HexColor) : ICommand<Result<SoilTypeResponse>>;