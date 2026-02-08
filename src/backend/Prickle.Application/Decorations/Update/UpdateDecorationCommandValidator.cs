namespace Prickle.Application.Decorations.Update;

internal sealed class UpdateDecorationCommandValidator : AbstractValidator<UpdateDecorationCommand>
{
    public UpdateDecorationCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Decoration ID cannot be empty.");

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
