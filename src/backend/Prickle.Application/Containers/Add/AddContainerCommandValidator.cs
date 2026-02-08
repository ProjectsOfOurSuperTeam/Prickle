using FluentValidation;

namespace Prickle.Application.Containers.Add;

internal sealed class AddContainerCommandValidator : AbstractValidator<AddContainerCommand>
{
    public AddContainerCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(command => command.Description)
            .MaximumLength(500);

        RuleFor(command => command.Volume)
            .GreaterThan(0);

        RuleFor(command => command.ImageUrl)
            .MaximumLength(2048);

        RuleFor(command => command.ImageIsometricUrl)
            .MaximumLength(2048);
    }
}