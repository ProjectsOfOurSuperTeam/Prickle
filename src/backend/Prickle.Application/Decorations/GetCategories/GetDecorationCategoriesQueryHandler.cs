using Prickle.Domain.Decorations;

namespace Prickle.Application.Decorations.GetCategories;

internal sealed class GetDecorationCategoriesQueryHandler
    : IQueryHandler<GetDecorationCategoriesQuery, Result<DecorationCategoriesResponse>>
{
    public ValueTask<Result<DecorationCategoriesResponse>> Handle(GetDecorationCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = DecorationCategory.List
            .Select(c => new DecorationCategoryResponse(c.Value, c.Name))
            .ToList();
        return ValueTask.FromResult(Result.Success(new DecorationCategoriesResponse(categories)));
    }
}
