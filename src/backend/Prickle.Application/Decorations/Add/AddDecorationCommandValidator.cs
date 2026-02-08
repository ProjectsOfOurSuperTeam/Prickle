namespace Prickle.Application.Decorations.Add;

internal sealed class AddDecorationCommandValidator : AbstractValidator<AddDecorationCommand>
{
    public AddDecorationCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(command => command.Description)
            .MaximumLength(500);

        RuleFor(command => command.ImageUrl)
            .MaximumLength(2048);

        RuleFor(command => command.ImageIsometricUrl)
            .MaximumLength(2048);
    }
}
