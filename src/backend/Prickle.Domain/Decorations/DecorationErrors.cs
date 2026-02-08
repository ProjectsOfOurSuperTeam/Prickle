
namespace Prickle.Domain.Decorations;

public static class DecorationErrors
{
    public static readonly Error EmptyName = Error.Problem(
        "Decoration.EmptyName",
        "Decoration name cannot be empty");

    public static Error NotFound(Guid id) => Error.Problem(
        "Decoration.NotFound",
        $"Decoration with ID '{id}' was not found"
    );

    public static Error AlreadyExists(string name) => Error.Problem(
        "Decoration.AlreadyExists",
        $"Decoration with name '{name}' already exists"
    );

    public static Error FailedToCreate(string name) => Error.Problem(
        "Decoration.FailedToCreate",
        $"Failed to create decoration with name '{name}'"
    );

    public static Error InvalidCategory(int category) => Error.Problem(
        "Decoration.InvalidCategory",
        $"Invalid decoration category '{category}'"
    );
}