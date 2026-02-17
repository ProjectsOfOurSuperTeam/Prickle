namespace Prickle.Application.Projects.GetAll;

internal sealed class GetAllProjectsQueryValidator : AbstractValidator<GetAllProjectsQuery>
{
    private static readonly string[] AcceptableSortFields =
    {
        "createdat", "id"
    };

    public GetAllProjectsQueryValidator()
    {
        RuleFor(x => x.SortField)
            .Must(x => x is null || AcceptableSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("You can only sort by 'createdat' or 'id'");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 25)
            .WithMessage("You can get between 1 and 25 projects per page");
    }
}