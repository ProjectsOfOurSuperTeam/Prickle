namespace Prickle.Application.Projects.Get;

public sealed record GetProjectQuery(Guid Id) : IQuery<Result<ProjectResponse>>;