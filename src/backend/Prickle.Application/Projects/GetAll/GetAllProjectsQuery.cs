namespace Prickle.Application.Projects.GetAll;

public sealed record GetAllProjectsQuery(Guid CallerUserId, Guid? UserId, bool? IsPublished) : PagedSortingOptions, IQuery<Result<ProjectsResponse>>;