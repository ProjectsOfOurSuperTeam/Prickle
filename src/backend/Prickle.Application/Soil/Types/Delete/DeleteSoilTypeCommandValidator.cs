namespace Prickle.Application.Soil.Types.Delete;

internal sealed class DeleteSoilTypeCommandValidator : AbstractValidator<DeleteSoilTypeCommand>
{
    public DeleteSoilTypeCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotNull()
            .GreaterThan(0);
    }
}
