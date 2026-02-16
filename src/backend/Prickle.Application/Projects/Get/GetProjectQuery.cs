namespace Prickle.Application.Projects.Get;

public sealed record GetProjectQuery(Guid Id, Guid UserId) : IQuery<Result<ProjectResponse>>;