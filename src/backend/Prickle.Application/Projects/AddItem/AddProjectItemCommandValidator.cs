using FluentValidation;

namespace Prickle.Application.Projects.AddItem;

internal sealed class AddProjectItemCommandValidator : AbstractValidator<AddProjectItemCommand>
{
    public AddProjectItemCommandValidator()
    {
        RuleFor(c => c.ProjectId).NotEmpty();
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.ItemId).NotEmpty();
    }
}