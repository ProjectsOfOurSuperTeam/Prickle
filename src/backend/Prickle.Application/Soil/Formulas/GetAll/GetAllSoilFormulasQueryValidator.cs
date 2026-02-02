namespace Prickle.Application.Soil.Formulas.GetAll;

internal sealed class GetAllSoilFormulasQueryValidator : AbstractValidator<GetAllSoilFormulasQuery>
{
    private static readonly string[] AcceptableSortFields =
    {
        "name", "id", "itemcount"
    };

    public GetAllSoilFormulasQueryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(255);

        RuleFor(x => x.SortField)
            .Must(x => x is null || AcceptableSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("You can only sort by 'name', 'id', or 'itemcount'");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 25)
            .WithMessage("You can get between 1 and 25 soil formulas per page");

        When(x => x.SoilTypeIds is not null, () =>
        {
            RuleFor(x => x.SoilTypeIds)
                .Must(ids => ids!.All(id => id > 0))
                .WithMessage("All soil type IDs must be greater than zero");

            RuleFor(x => x.SoilTypeIds)
                .Must(ids => ids!.Count() <= 10)
                .WithMessage("You can filter by maximum 10 soil types at once");
        });
    }
}
