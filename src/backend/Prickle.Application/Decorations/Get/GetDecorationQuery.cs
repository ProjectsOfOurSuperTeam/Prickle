namespace Prickle.Application.Decorations.Get;

public sealed record GetDecorationQuery(Guid Id) : IQuery<Result<DecorationResponse>>;