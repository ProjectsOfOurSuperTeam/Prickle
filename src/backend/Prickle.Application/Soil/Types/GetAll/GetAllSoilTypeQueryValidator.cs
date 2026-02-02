namespace Prickle.Application.Soil.Types.GetAll;

internal sealed class GetAllSoilTypeQueryValidator : AbstractValidator<GetAllSoilTypeQuery>
{
    private static readonly string[] AcceptableSortFields =
    {
        "name", "id"
    };

    public GetAllSoilTypeQueryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(255);

        RuleFor(x => x.SortField)
            .Must(x => x is null || AcceptableSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("You can only sort by 'name' or 'id'");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 25)
            .WithMessage("You can get between 1 and 25 soil types per page");
    }
}
