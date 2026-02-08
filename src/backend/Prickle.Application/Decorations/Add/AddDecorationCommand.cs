using Prickle.Domain.Decorations;

namespace Prickle.Application.Decorations.Add;

public sealed record AddDecorationCommand(
    string Name,
    string? Description,
    DecorationCategory Category,
    string? ImageUrl,
    string? ImageIsometricUrl) : ICommand<Result<DecorationResponse>>;