using Mediator;

namespace Prickle.Application.Soil.Types.Add;

public sealed record AddSoilTypeCommand(string Name) : ICommand<Result<SoilTypeResponse>>;