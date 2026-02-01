using Bogus;
using Prickle.Domain.Soil;

namespace Prickle.Unit.Tests.Domain.Soil;

public class SoilFormulaItemTests
{
    private readonly Faker _faker;

    public SoilFormulaItemTests()
    {
        _faker = new Faker();
    }

    [Fact]
    public void Create_ValidInputs_ShouldReturnSuccess()
    {
        // Arrange
        var soilTypeId = _faker.Random.Int(1, 100);
        var percentage = _faker.Random.Int(1, 100);
        var order = _faker.Random.Int(0, 10);

        // Act
        var result = SoilFormulaItem.Create(soilTypeId, percentage, order);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.SoilTypeId.ShouldBe(soilTypeId);
        result.Value.Percentage.ShouldBe(percentage);
        result.Value.Order.ShouldBe(order);
    }

    [Fact]
    public void Create_InvalidSoilTypeId_ShouldReturnFailure()
    {
        // Arrange
        var invalidSoilTypeId = 0;
        var percentage = _faker.Random.Int(1, 100);
        var order = _faker.Random.Int(0, 10);

        // Act
        var result = SoilFormulaItem.Create(invalidSoilTypeId, percentage, order);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SoilErrors.SoilFormulas.InvalidSoilTypeId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_InvalidPercentage_ShouldReturnFailure(int invalidPercentage)
    {
        // Arrange
        var soilTypeId = _faker.Random.Int(1, 100);
        var order = _faker.Random.Int(0, 10);

        // Act
        var result = SoilFormulaItem.Create(soilTypeId, invalidPercentage, order);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SoilErrors.SoilFormulas.InvalidPercentage);
    }

    [Fact]
    public void Create_InvalidOrder_ShouldReturnFailure()
    {
        // Arrange
        var soilTypeId = _faker.Random.Int(1, 100);
        var percentage = _faker.Random.Int(1, 100);
        var invalidOrder = -1;

        // Act
        var result = SoilFormulaItem.Create(soilTypeId, percentage, invalidOrder);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SoilErrors.SoilFormulas.InvalidOrder);
    }
}