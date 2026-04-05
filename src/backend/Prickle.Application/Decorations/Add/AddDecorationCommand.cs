using Prickle.Domain.Decorations;
using Prickle.Domain.Projects;

namespace Prickle.Application.Decorations.Add;

public sealed record AddDecorationCommand(
    string Name,
    string? Description,
    DecorationCategory Category,
    string? ImageUrl,
    string? ImageIsometricUrl,
    ProjectItemSize ItemMaxSize) : ICommand<Result<DecorationResponse>>;