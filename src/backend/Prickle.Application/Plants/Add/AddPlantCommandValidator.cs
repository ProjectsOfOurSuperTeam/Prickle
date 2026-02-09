using FluentValidation;

namespace Prickle.Application.Plants.Add;

internal sealed class AddPlantCommandValidator : AbstractValidator<AddPlantCommand>
{
    public AddPlantCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.NameLatin).NotEmpty();
        RuleFor(c => c.SoilFormulaId).NotEmpty();
    }
}