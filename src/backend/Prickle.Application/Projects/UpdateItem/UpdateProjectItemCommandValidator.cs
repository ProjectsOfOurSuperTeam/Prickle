namespace Prickle.Application.Projects.UpdateItem;

internal sealed class UpdateProjectItemCommandValidator : AbstractValidator<UpdateProjectItemCommand>
{
    public UpdateProjectItemCommandValidator()
    {
        RuleFor(c => c.ProjectId).NotEmpty();
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.ItemId).NotEmpty();
    }
}