using FluentValidation;

namespace Prickle.Application.Plants.GetAll;

internal sealed class GetAllPlantsQueryValidator : AbstractValidator<GetAllPlantsQuery>
{
    private static readonly string[] AcceptableSortFields =
    {
        "name", "namelatin", "id"
    };

    public GetAllPlantsQueryValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(255);

        RuleFor(x => x.SortField)
            .Must(x => x is null || AcceptableSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("You can only sort by 'name', 'namelatine' or 'id'");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 25)
            .WithMessage("You can get between 1 and 25 plants per page");
    }
}