namespace Prickle.Application.Decorations.Delete;

public sealed record DeleteDecorationCommand(Guid Id) : ICommand<Result>;