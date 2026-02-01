using Bogus;
using Prickle.Domain.Soil;

namespace Prickle.Unit.Tests.Domain.Soil;

public class SoilFormulasTests
{
    private readonly Faker _faker;

    public SoilFormulasTests()
    {
        _faker = new Faker();
    }

    [Fact]
    public void Create_ValidInputs_ShouldReturnSuccess()
    {
        // Arrange
        var name = _faker.Lorem.Word();
        var formulaItems = new List<SoilFormulaItem>
        {
            SoilFormulaItem.Create(1, 50, 0).Value,
            SoilFormulaItem.Create(2, 50, 1).Value
        };

        // Act
        var result = SoilFormulas.Create(name, formulaItems);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(name);
        result.Value.Formula.Count.ShouldBe(2);
        result.Value.Formula.Sum(x => x.Percentage).ShouldBe(100);
    }

    [Fact]
    public void Create_EmptyName_ShouldReturnFailure()
    {
        // Arrange
        var emptyName = " ";
        var formulaItems = new List<SoilFormulaItem>
        {
            SoilFormulaItem.Create(1, 50, 0).Value
        };

        // Act
        var result = SoilFormulas.Create(emptyName, formulaItems);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SoilErrors.SoilFormulas.SoilFormulasNameEmpty);
    }

    [Fact]
    public void Create_NullFormulaItems_ShouldReturnFailure()
    {
        // Arrange
        var name = _faker.Lorem.Word();
        List<SoilFormulaItem>? formulaItems = null;

        // Act
        var result = SoilFormulas.Create(name, formulaItems);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SoilErrors.SoilFormulas.SoilFormulasFormulaEmpty);
    }

    [Fact]
    public void Create_EmptyFormulaItems_ShouldReturnFailure()
    {
        // Arrange
        var name = _faker.Lorem.Word();
        var formulaItems = new List<SoilFormulaItem>();

        // Act
        var result = SoilFormulas.Create(name, formulaItems);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SoilErrors.SoilFormulas.SoilFormulasFormulaEmpty);
    }

    [Fact]
    public void Create_PercentageExceeds100_ShouldReturnFailure()
    {
        // Arrange
        var name = _faker.Lorem.Word();
        var formulaItems = new List<SoilFormulaItem>
        {
            SoilFormulaItem.Create(1, 60, 0).Value,
            SoilFormulaItem.Create(2, 50, 1).Value
        };

        // Act
        var result = SoilFormulas.Create(name, formulaItems);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SoilErrors.SoilFormulas.SoilFormulasPercentageInvalid);
    }

    [Fact]
    public void Create_DuplicateOrderAndSoilTypeId_ShouldAggregatePercentages()
    {
        // Arrange
        var name = _faker.Lorem.Word();
        var formulaItems = new List<SoilFormulaItem>
        {
            SoilFormulaItem.Create(1, 30, 0).Value,
            SoilFormulaItem.Create(1, 20, 0).Value,
            SoilFormulaItem.Create(2, 50, 1).Value
        };

        // Act
        var result = SoilFormulas.Create(name, formulaItems);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Formula.Count.ShouldBe(2);
        result.Value.Formula.First(x => x.SoilTypeId == 1).Percentage.ShouldBe(50);
        result.Value.Formula.First(x => x.SoilTypeId == 2).Percentage.ShouldBe(50);
    }
}