namespace Prickle.Application.Soil.Types.Add;

internal sealed class AddSoilTypeCommandValidator : AbstractValidator<AddSoilTypeCommand>
{
    public AddSoilTypeCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(255);
    }
}
