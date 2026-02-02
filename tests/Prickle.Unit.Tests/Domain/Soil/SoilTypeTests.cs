using Bogus;
using Prickle.Domain.Soil;

namespace Prickle.Unit.Tests.Domain.Soil;

public class SoilTypeTests
{
    private readonly Faker _faker;

    public SoilTypeTests()
    {
        _faker = new Faker();
    }

    [Fact]
    public void Create_ValidName_ShouldReturnSuccess()
    {
        // Arrange
        var validName = _faker.Lorem.Word();

        // Act
        var result = SoilType.Create(validName);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(validName);
    }

    [Fact]
    public void Create_EmptyName_ShouldReturnFailure()
    {
        // Arrange
        var emptyName = " ";

        // Act
        var result = SoilType.Create(emptyName);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SoilErrors.SoilType.EmptyName);
    }
}