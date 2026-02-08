using Microsoft.EntityFrameworkCore;
using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Decorations;

namespace Prickle.Application.Decorations.Delete;

internal sealed class DeleteDecorationCommandHandler
    (IApplicationDbContext dbContext)
    : ICommandHandler<DeleteDecorationCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteDecorationCommand command, CancellationToken cancellationToken)
    {
        var decoration = await dbContext.Decorations.FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken);
        if (decoration is null)
        {
            return Result.Failure(DecorationErrors.NotFound(command.Id));
        }

        dbContext.Decorations.Remove(decoration);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
