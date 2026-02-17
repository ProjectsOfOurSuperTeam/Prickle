namespace Prickle.Application.Plants.Update;

internal sealed class UpdatePlantCommandValidator : AbstractValidator<UpdatePlantCommand>
{
    public UpdatePlantCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty();
        RuleFor(c => c.NameLatin).NotEmpty();
        RuleFor(c => c.SoilFormulaId).NotEmpty();
    }
}