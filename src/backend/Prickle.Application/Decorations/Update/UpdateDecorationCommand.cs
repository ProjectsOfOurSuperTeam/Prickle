using Prickle.Domain.Decorations;

namespace Prickle.Application.Decorations.Update;

public sealed record UpdateDecorationCommand(
    Guid Id,
    string Name,
    string? Description,
    DecorationCategory Category,
    string? ImageUrl,
    string? ImageIsometricUrl) : ICommand<Result<DecorationResponse>>;
