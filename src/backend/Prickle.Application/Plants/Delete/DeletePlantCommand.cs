namespace Prickle.Application.Plants.Delete;

public sealed record DeletePlantCommand(Guid Id) : ICommand<Result>;