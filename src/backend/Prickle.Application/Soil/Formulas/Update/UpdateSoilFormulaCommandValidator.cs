namespace Prickle.Application.Soil.Formulas.Update;

internal sealed class UpdateSoilFormulaCommandValidator : AbstractValidator<UpdateSoilFormulaCommand>
{
    public UpdateSoilFormulaCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Soil formula ID cannot be empty.");

        RuleFor(command => command.NewName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(command => command.FormulaItemDTOs)
            .NotEmpty()
            .WithMessage("Formula must contain at least one item.");

        RuleForEach(command => command.FormulaItemDTOs)
            .ChildRules(item =>
            {
                item.RuleFor(dto => dto.SoilTypeId)
                    .GreaterThan(0)
                    .WithMessage("Soil type ID must be greater than zero.");

                item.RuleFor(dto => dto.Percentage)
                    .InclusiveBetween(1, 100)
                    .WithMessage("Percentage must be between 1 and 100.");

                item.RuleFor(dto => dto.Order)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Order must be zero or a positive integer.");
            });
    }
}
