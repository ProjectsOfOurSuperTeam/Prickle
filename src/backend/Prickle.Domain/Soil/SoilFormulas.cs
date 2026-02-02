namespace Prickle.Domain.Soil;

public sealed class SoilFormulas : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    private readonly List<SoilTypeSoilFormula> _formula = new();
    public IReadOnlyCollection<SoilTypeSoilFormula> Formula => _formula.AsReadOnly();
    private SoilFormulas() { }

    private SoilFormulas(Guid id, string name, List<SoilTypeSoilFormula> formula)
    {
        Id = id;
        Name = name;
        _formula = formula;
    }

    public static Result<SoilFormulas> Create(string name, List<SoilFormulaItem> formulaItems)
    {
        var normalizedName = name.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Result.Failure<SoilFormulas>(SoilErrors.SoilFormulas.SoilFormulasNameEmpty);
        }

        if (formulaItems is null || formulaItems.Count == 0)
        {
            return Result.Failure<SoilFormulas>(SoilErrors.SoilFormulas.SoilFormulasFormulaEmpty);
        }

        var processedItems = formulaItems
        .GroupBy(item => new { item.Order, item.SoilTypeId })
        .Select(group => new
        {
            group.Key.Order,
            group.Key.SoilTypeId,
            Percentage = group.Sum(x => x.Percentage)
        })
        .ToList();

        if (processedItems.Sum(x => x.Percentage) > 100)
        {
            return Result.Failure<SoilFormulas>(SoilErrors.SoilFormulas.SoilFormulasPercentageInvalid);
        }

        var newItemId = Guid.NewGuid();

        var finalEntities = processedItems
        .Select(x => new SoilTypeSoilFormula(newItemId, x.SoilTypeId, x.Percentage, x.Order))
        .ToList();
        var soilFormulas = new SoilFormulas(newItemId, normalizedName, finalEntities);
        return Result.Success(soilFormulas);
    }

    public Result<SoilFormulas> Update(string newName, List<SoilFormulaItem> formulaItems)
    {
        var normalizedName = newName.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return Result.Failure<SoilFormulas>(SoilErrors.SoilFormulas.SoilFormulasNameEmpty);
        }

        if (formulaItems is null || formulaItems.Count == 0)
        {
            return Result.Failure<SoilFormulas>(SoilErrors.SoilFormulas.SoilFormulasFormulaEmpty);
        }

        var processedItems = formulaItems
        .GroupBy(item => new { item.Order, item.SoilTypeId })
        .Select(group => new
        {
            group.Key.Order,
            group.Key.SoilTypeId,
            Percentage = group.Sum(x => x.Percentage)
        })
        .ToList();

        if (processedItems.Sum(x => x.Percentage) > 100)
        {
            return Result.Failure<SoilFormulas>(SoilErrors.SoilFormulas.SoilFormulasPercentageInvalid);
        }

        Name = normalizedName;
        _formula.Clear();
        var finalEntities = processedItems
        .Select(x => new SoilTypeSoilFormula(Id, x.SoilTypeId, x.Percentage, x.Order))
        .ToList();
        _formula.AddRange(finalEntities);

        return Result.Success(this);
    }
}
