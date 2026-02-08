using FluentValidation;

namespace Prickle.Application.Containers.Update;

internal sealed class UpdateContainerCommandValidator : AbstractValidator<UpdateContainerCommand>
{
    public UpdateContainerCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

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