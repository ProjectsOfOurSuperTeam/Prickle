namespace Prickle.Application.Containers.GetAll;

internal sealed class GetAllContainersQueryValidator : AbstractValidator<GetAllContainersQuery>
{
    private static readonly string[] AcceptableSortFields =
    {
        "name", "id", "volume"
    };

    public GetAllContainersQueryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(255);

        RuleFor(x => x.SortField)
            .Must(x => x is null || AcceptableSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("You can only sort by 'name', 'id', or 'volume'");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 25)
            .WithMessage("You can get between 1 and 25 containers per page");
    }
}