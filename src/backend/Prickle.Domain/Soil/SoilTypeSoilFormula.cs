namespace Prickle.Domain.Soil;

public sealed class SoilTypeSoilFormula
{
    public Guid SoilFormulaId { get; private set; }
    public int SoilTypeId { get; private set; }
    public int Order { get; private set; }
    public int Percentage { get; private set; }

    private SoilTypeSoilFormula()
    {
    }
    internal SoilTypeSoilFormula(Guid soilFormulaId, int soilTypeId, int percentage, int order) : this()
    {
        SoilFormulaId = soilFormulaId;
        SoilTypeId = soilTypeId;
        Percentage = percentage;
        Order = order;
    }
}
