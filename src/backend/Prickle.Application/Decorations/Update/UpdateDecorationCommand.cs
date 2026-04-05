using Prickle.Domain.Decorations;
using Prickle.Domain.Projects;

namespace Prickle.Application.Decorations.Update;

public sealed record UpdateDecorationCommand(
    Guid Id,
    string Name,
    string? Description,
    DecorationCategory Category,
    string? ImageUrl,
    string? ImageIsometricUrl,
    ProjectItemSize ItemMaxSize) : ICommand<Result<DecorationResponse>>;
