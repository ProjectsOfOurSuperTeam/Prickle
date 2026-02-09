namespace Prickle.Application.Plants.Get;

public sealed record GetPlantQuery(Guid Id) : IQuery<Result<PlantResponse>>;