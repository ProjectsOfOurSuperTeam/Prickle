namespace Prickle.Domain.Projects;

public static class ProjectErrors
{
    public static readonly Error EmptyUserId = Error.Problem(
        "Project.EmptyUserId",
        "User ID cannot be empty");

    public static readonly Error EmptyContainerId = Error.Problem(
        "Project.EmptyContainerId",
        "Container ID cannot be empty");

    public static readonly Error EmptyProjectId = Error.Problem(
        "ProjectItem.EmptyProjectId",
        "Project ID cannot be empty");

    public static readonly Error EmptyItemId = Error.Problem(
        "ProjectItem.EmptyItemId",
        "Item ID cannot be empty");

    public static Error NotFound(Guid id) => Error.Problem(
        "Project.NotFound",
        $"Project with ID '{id}' was not found");

    public static Error ProjectItemNotFound(Guid itemId) => Error.Problem(
        "ProjectItem.NotFound",
        $"Project item with ID '{itemId}' was not found");

    public static Error UserNotOwner(Guid userId) => Error.Problem(
        "Project.UserNotOwner",
        $"User '{userId}' is not the owner of this project");

    public static Error AlreadyPublished(Guid projectId) => Error.Problem(
        "Project.AlreadyPublished",
        $"Project '{projectId}' is already published");

    public static Error NotPublished(Guid projectId) => Error.Problem(
        "Project.NotPublished",
        $"Project '{projectId}' is not published");
}