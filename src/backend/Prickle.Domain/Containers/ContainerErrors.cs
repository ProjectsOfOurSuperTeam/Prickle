namespace Prickle.Domain.Containers;

public static class ContainerErrors
{
    public static readonly Error EmptyName = Error.Problem(
    "Container.EmptyName",
    "Container name cannot be empty");

    public static readonly Error InvalidVolume = Error.Problem(
        "Container.InvalidVolume",
        "Container volume must be greater than zero."
    );

    public static Error NotFound(Guid id) => Error.Problem(
        "Container.NotFound",
        $"Container with ID '{id}' was not found"
    );

    public static Error AlreadyExists(string name) => Error.Problem(
        "Container.AlreadyExists",
        $"Container with name '{name}' already exists"
    );

    public static Error FailedToCreate(string name) => Error.Problem(
        "Container.FailedToCreate",
        $"Failed to create container with name '{name}'"
    );
}
