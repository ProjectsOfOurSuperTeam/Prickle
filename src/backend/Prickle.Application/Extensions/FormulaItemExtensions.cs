using Prickle.Application.Soil.Formulas;
using Prickle.Domain.Soil;

namespace Prickle.Application.Extensions;

internal static class FormulaItemExtensions
{
    extension(AddSoilFormulaItemDTO dto)
    {
        public Result<SoilFormulaItem> ToSoilFormulaItem()
        {
            return SoilFormulaItem.Create(dto.SoilTypeId, dto.Percentage, dto.Order);
        }
    }

    extension(IEnumerable<AddSoilFormulaItemDTO> dtos)
    {
        public Result<List<SoilFormulaItem>> ToSoilFormulaItems()
        {
            var items = new List<SoilFormulaItem>();

            foreach (var dto in dtos)
            {
                var result = dto.ToSoilFormulaItem();
                if (result.IsFailure)
                {
                    return Result.Failure<List<SoilFormulaItem>>(result.Error);
                }

                items.Add(result.Value);
            }

            return Result.Success(items);
        }
    }
}