using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Soil.Formulas;
using Prickle.Application.Soil.Types;
using SoilFormulaEndpoints = Prickle.Api.Endpoints.Soil.Formulas;
using SoilTypeEndpoints = Prickle.Api.Endpoints.Soil.Types;

namespace Prickle.Integration.Tests.Api.SoilFormulaEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class AddSoilFormulaEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public AddSoilFormulaEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task AddSoilFormula_ValidRequest_ShouldReturnCreatedWithSoilFormula()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types first
        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);

        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 60, 0),
            new(soilType2.Id, 40, 1)
        };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(_faker.Lorem.Word(), formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();
        soilFormulaResponse.Id.ShouldNotBe(Guid.Empty);
        soilFormulaResponse.Name.ShouldBe(newSoilFormula.Name);
        soilFormulaResponse.Items.Count().ShouldBe(2);

        var items = soilFormulaResponse.Items.ToList();
        items[0].SoilType.Id.ShouldBe(soilType1.Id);
        items[0].Percentage.ShouldBe(60);
        items[0].Order.ShouldBe(0);
        items[1].SoilType.Id.ShouldBe(soilType2.Id);
        items[1].Percentage.ShouldBe(40);
        items[1].Order.ShouldBe(1);

        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddSoilFormula_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(string.Empty, formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddSoilFormula_NameTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var longName = _faker.Random.String2(256); // Max is 255
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(longName, formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddSoilFormula_DuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var formulaName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);

        // Act - Add the soil formula first time
        var firstResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act - Try to add the same soil formula again
        var secondResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddSoilFormula_DuplicateNameDifferentCasing_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var baseFormulaName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaNameLower = baseFormulaName.ToLowerInvariant();
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };

        var firstRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaNameLower, formulaItems);
        var secondRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaNameLower.ToUpperInvariant(), formulaItems);

        // Act - Add the soil formula with lowercase
        var firstResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, firstRequest, cts);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act - Try to add with uppercase
        var secondResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, secondRequest, cts);

        // Assert
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddSoilFormula_NameWithWhitespace_ShouldTrimAndReturnCreated()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var formulaName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest($"  {formulaName}  ", formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();
        soilFormulaResponse.Name.ShouldBe(newSoilFormula.Name.Trim());
    }

    [Fact]
    public async Task AddSoilFormula_EmptyFormulaItems_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var formulaItems = new List<SoilFormulaItemDTO>();
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(_faker.Lorem.Word(), formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task AddSoilFormula_InvalidSoilTypeId_ShouldReturnBadRequest(int invalidSoilTypeId)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var formulaItems = new List<SoilFormulaItemDTO> { new(invalidSoilTypeId, 100, 0) };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(_faker.Lorem.Word(), formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(200)]
    public async Task AddSoilFormula_InvalidPercentage_ShouldReturnBadRequest(int invalidPercentage)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, invalidPercentage, 0) };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(_faker.Lorem.Word(), formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public async Task AddSoilFormula_NegativeOrder_ShouldReturnBadRequest(int invalidOrder)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, invalidOrder) };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(_faker.Lorem.Word(), formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddSoilFormula_MultipleItemsWithSameSoilType_ShouldReturnCreated()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);

        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 50, 0),
            new(soilType2.Id, 30, 1),
            new(soilType3.Id, 20, 2)
        };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(_faker.Lorem.Word(), formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();
        soilFormulaResponse.Items.Count().ShouldBe(3);

        var items = soilFormulaResponse.Items.OrderBy(i => i.Order).ToList();
        items[0].Percentage.ShouldBe(50);
        items[1].Percentage.ShouldBe(30);
        items[2].Percentage.ShouldBe(20);
    }

    [Fact]
    public async Task AddSoilFormula_SingleItem_ShouldReturnCreated()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest($"{_faker.Lorem.Word()}-{Guid.NewGuid():N}", formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();
        soilFormulaResponse.Items.Count().ShouldBe(1);
        soilFormulaResponse.Items.First().Percentage.ShouldBe(100);
    }

    [Theory]
    [InlineData("Premium Mix")]
    [InlineData("Custom Blend 123")]
    [InlineData("Mix-A1")]
    public async Task AddSoilFormula_VariousValidNames_ShouldReturnCreated(string formulaNamePrefix)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Make the name unique by adding a GUID suffix to avoid conflicts with other tests
        var formulaName = $"{formulaNamePrefix}-{Guid.NewGuid():N}";
        var soilType = await CreateSoilType(client, cts);
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();
        soilFormulaResponse.Name.ShouldBe(formulaName);
    }

    [Fact]
    public async Task AddSoilFormula_ComplexFormula_ShouldReturnCreatedWithCorrectOrdering()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);
        var soilType4 = await CreateSoilType(client, cts);

        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 40, 0),
            new(soilType2.Id, 25, 1),
            new(soilType3.Id, 20, 2),
            new(soilType4.Id, 15, 3)
        };
        var newSoilFormula = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest($"Complex-{Guid.NewGuid():N}", formulaItems);

        // Act
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, newSoilFormula, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();
        soilFormulaResponse.Items.Count().ShouldBe(4);

        var items = soilFormulaResponse.Items.OrderBy(i => i.Order).ToList();
        items[0].Order.ShouldBe(0);
        items[1].Order.ShouldBe(1);
        items[2].Order.ShouldBe(2);
        items[3].Order.ShouldBe(3);

        // Verify percentages
        items[0].Percentage.ShouldBe(40);
        items[1].Percentage.ShouldBe(25);
        items[2].Percentage.ShouldBe(20);
        items[3].Percentage.ShouldBe(15);
    }

    private async Task<SoilTypeResponse> CreateSoilType(HttpClient client, CancellationToken cancellationToken)
    {
        var soilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var soilTypeRequest = new SoilTypeEndpoints.Add.AddSoilTypeRequest(soilTypeName);
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, soilTypeRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        var soilType = await response.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cancellationToken);
        return soilType!;
    }
}
