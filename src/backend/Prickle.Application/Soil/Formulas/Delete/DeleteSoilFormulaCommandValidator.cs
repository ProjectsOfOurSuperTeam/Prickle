namespace Prickle.Application.Soil.Formulas.Delete;

internal sealed class DeleteSoilFormulaCommandValidator : AbstractValidator<DeleteSoilFormulaCommand>
{
    public DeleteSoilFormulaCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Soil formula ID cannot be empty.");
    }
}
