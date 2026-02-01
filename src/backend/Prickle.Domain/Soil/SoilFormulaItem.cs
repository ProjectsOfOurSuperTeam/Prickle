namespace Prickle.Domain.Soil;

public sealed class SoilFormulaItem
{
    public int SoilTypeId { get; private set; }
    public int Percentage { get; private set; }
    public int Order { get; private set; }

    private SoilFormulaItem()
    {
    }
    private SoilFormulaItem(int soilTypeId, int percentage, int order) : this()
    {
        SoilTypeId = soilTypeId;
        Percentage = percentage;
        Order = order;
    }

    public static Result<SoilFormulaItem> Create(int soilTypeId, int percentage, int order)
    {
        if (soilTypeId <= 0)
        {
            return Result.Failure<SoilFormulaItem>(SoilErrors.SoilFormulas.InvalidSoilTypeId);
        }

        if (percentage <= 0 || percentage > 100)
        {
            return Result.Failure<SoilFormulaItem>(SoilErrors.SoilFormulas.InvalidPercentage);
        }

        if (order < 0)
        {
            return Result.Failure<SoilFormulaItem>(SoilErrors.SoilFormulas.InvalidOrder);
        }

        var formulaItem = new SoilFormulaItem(soilTypeId, percentage, order);
        return Result.Success(formulaItem);
    }
}
