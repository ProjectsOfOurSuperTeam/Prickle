namespace Prickle.Application.Projects.GetAll;

public sealed record GetAllProjectsQuery(Guid? UserId, bool? IsPublished) : PagedSortingOptions, IQuery<Result<ProjectsResponse>>;