using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Soil;

namespace Prickle.Application.Soil.Formulas.Delete;

internal sealed class DeleteSoilFormulaCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<DeleteSoilFormulaCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteSoilFormulaCommand command, CancellationToken cancellationToken)
    {
        var soilFormula = await dbContext.SoilFormulas
            .Include(sf => sf.Formula)
            .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (soilFormula is null)
        {
            return Result.Failure(SoilErrors.SoilFormulas.NotFound(command.Id));
        }

        dbContext.SoilFormulas.Remove(soilFormula);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
