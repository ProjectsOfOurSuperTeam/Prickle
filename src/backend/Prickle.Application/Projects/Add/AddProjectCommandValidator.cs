using FluentValidation;

namespace Prickle.Application.Projects.Add;

internal sealed class AddProjectCommandValidator : AbstractValidator<AddProjectCommand>
{
    public AddProjectCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.ContainerId).NotEmpty();
    }
}