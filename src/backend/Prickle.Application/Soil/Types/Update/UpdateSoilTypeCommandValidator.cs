namespace Prickle.Application.Soil.Types.Update;

internal sealed class UpdateSoilTypeCommandValidator : AbstractValidator<UpdateSoilTypeCommand>
{
    public UpdateSoilTypeCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotNull()
            .GreaterThan(0);

        RuleFor(command => command.NewName)
            .NotEmpty()
            .MaximumLength(255);
    }
}
