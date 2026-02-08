using Prickle.Application.Abstractions.Database;
using Prickle.Domain.Decorations;

namespace Prickle.Application.Decorations.Get;

internal sealed class GetDecorationQueryHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetDecorationQuery, Result<DecorationResponse>>
{
    public async ValueTask<Result<DecorationResponse>> Handle(GetDecorationQuery query, CancellationToken cancellationToken)
    {
        var decoration = dbContext.Decorations.FirstOrDefault(d => d.Id == query.Id);
        if (decoration is null)
        {
            return Result.Failure<DecorationResponse>(DecorationErrors.NotFound(query.Id));
        }

        var response = new DecorationResponse
        {
            Id = decoration.Id,
            Name = decoration.Name,
            Description = decoration.Description,
            Category = decoration.Category,
            ImageUrl = decoration.ImageUrl,
            ImageIsometricUrl = decoration.ImageIsometricUrl
        };
        return Result.Success(response);
    }
}
