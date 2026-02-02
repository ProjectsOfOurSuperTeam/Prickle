using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Soil.Formulas;
using Prickle.Application.Soil.Types;
using SoilFormulaEndpoints = Prickle.Api.Endpoints.Soil.Formulas;
using SoilTypeEndpoints = Prickle.Api.Endpoints.Soil.Types;

namespace Prickle.Integration.Tests.Api.SoilFormulaEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class UpdateSoilFormulaEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;
    private readonly ITestOutputHelper _outputHelper;

    public UpdateSoilFormulaEndpointTest(AppHostFixture appHostFactory, ITestOutputHelper outputHelper) : base(appHostFactory)
    {
        _faker = new Faker();
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task UpdateSoilFormula_ValidRequest_ShouldReturnOkWithUpdatedSoilFormula()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types
        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);

        // Create a soil formula first
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 60, 0),
            new(soilType2.Id, 40, 1)
        };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(originalName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        var createdSoilFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var soilType3 = await CreateSoilType(client, cts);
        var newFormulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType3.Id, 100, 0)
        };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(newName, newFormulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdSoilFormula!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedSoilFormula = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        updatedSoilFormula.ShouldNotBeNull();
        updatedSoilFormula.Id.ShouldBe(createdSoilFormula.Id);
        updatedSoilFormula.Name.ShouldBe(newName);
        updatedSoilFormula.Items.Count().ShouldBe(1);
        updatedSoilFormula.Items.First().SoilType.Id.ShouldBe(soilType3.Id);
        updatedSoilFormula.Items.First().Percentage.ShouldBe(100);
    }

    [Fact]
    public async Task UpdateSoilFormula_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var nonExistentId = Guid.NewGuid();

        // Act
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest($"{_faker.Lorem.Word()}-{Guid.NewGuid():N}", formulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", nonExistentId.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoilFormula_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil formula
        var soilType = await CreateSoilType(client, cts);
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(string.Empty, formulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoilFormula_NameTooLong_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil formula
        var soilType = await CreateSoilType(client, cts);
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act
        var longName = _faker.Random.String2(256); // Max is 255
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(longName, formulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoilFormula_DuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create two soil formulas
        var firstName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var secondName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";

        var firstFormula = await CreateSoilFormula(client, soilType, cts, firstName);
        var secondFormula = await CreateSoilFormula(client, soilType, cts, secondName);

        // Act - Try to update second soil formula to first soil formula's name
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(firstName, formulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", secondFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoilFormula_DuplicateNameDifferentCasing_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create two soil formulas with unique names
        var baseFirstName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var firstName = baseFirstName.ToLowerInvariant();
        var secondName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";

        var firstFormula = await CreateSoilFormula(client, soilType, cts, firstName);
        var secondFormula = await CreateSoilFormula(client, soilType, cts, secondName);

        // Act - Try to update second soil formula to first soil formula's name (different casing)
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(firstName.ToUpperInvariant(), formulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", secondFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoilFormula_UpdateToSameName_ShouldReturnOk()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create a soil formula
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createdFormula = await CreateSoilFormula(client, soilType, cts, originalName);

        // Act - Update to the same name
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(originalName, formulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedSoilFormula = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        updatedSoilFormula.ShouldNotBeNull();
        updatedSoilFormula.Name.ShouldBe(originalName);
    }

    [Fact]
    public async Task UpdateSoilFormula_UpdateWithWhitespace_ShouldReturnOk()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create a soil formula
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest($"  {newName}  ", formulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);
        var content = await response.Content.ReadAsStringAsync(cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _outputHelper.WriteLine(content);
        var updatedSoilFormula = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        updatedSoilFormula.ShouldNotBeNull();
        updatedSoilFormula.Name.ShouldBe(newName.Trim());
    }

    [Fact]
    public async Task UpdateSoilFormula_EmptyFormulaItems_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create a soil formula
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var emptyFormulaItems = new List<SoilFormulaItemDTO>();
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(newName, emptyFormulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task UpdateSoilFormula_InvalidSoilTypeId_ShouldReturnBadRequest(int invalidSoilTypeId)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create a soil formula
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(invalidSoilTypeId, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(newName, formulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(200)]
    public async Task UpdateSoilFormula_InvalidPercentage_ShouldReturnBadRequest(int invalidPercentage)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create a soil formula
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, invalidPercentage, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(newName, formulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public async Task UpdateSoilFormula_NegativeOrder_ShouldReturnBadRequest(int invalidOrder)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create a soil formula
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, invalidOrder) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(newName, formulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoilFormula_UpdateFromMultipleToSingleItem_ShouldReturnOk()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);

        // Create a soil formula with multiple items
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 50, 0),
            new(soilType2.Id, 30, 1),
            new(soilType3.Id, 20, 2)
        };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(originalName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        var createdFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        // Act - Update to a single item
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var newFormulaItems = new List<SoilFormulaItemDTO> { new(soilType1.Id, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(newName, newFormulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula!.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedFormula = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        updatedFormula.ShouldNotBeNull();
        updatedFormula.Items.Count().ShouldBe(1);
        updatedFormula.Items.First().Percentage.ShouldBe(100);
    }

    [Fact]
    public async Task UpdateSoilFormula_UpdateFromSingleToMultipleItems_ShouldReturnOk()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);

        // Create a soil formula with single item
        var createdFormula = await CreateSoilFormula(client, soilType1, cts);

        // Act - Update to multiple items
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var newFormulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 40, 0),
            new(soilType2.Id, 35, 1),
            new(soilType3.Id, 25, 2)
        };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(newName, newFormulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedFormula = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        updatedFormula.ShouldNotBeNull();
        updatedFormula.Items.Count().ShouldBe(3);

        var items = updatedFormula.Items.OrderBy(i => i.Order).ToList();
        items[0].Percentage.ShouldBe(40);
        items[1].Percentage.ShouldBe(35);
        items[2].Percentage.ShouldBe(25);
    }

    [Fact]
    public async Task UpdateSoilFormula_ComplexUpdate_ShouldReturnOkWithCorrectOrdering()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);

        // Create a soil formula
        var createdFormula = await CreateSoilFormula(client, soilType1, cts);

        // Act - Update with complex formula
        var newName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var soilType3 = await CreateSoilType(client, cts);
        var soilType4 = await CreateSoilType(client, cts);

        var newFormulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 30, 0),
            new(soilType2.Id, 25, 1),
            new(soilType3.Id, 25, 2),
            new(soilType4.Id, 20, 3)
        };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(newName, newFormulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedFormula = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        updatedFormula.ShouldNotBeNull();
        updatedFormula.Items.Count().ShouldBe(4);

        var items = updatedFormula.Items.OrderBy(i => i.Order).ToList();
        items[0].Order.ShouldBe(0);
        items[1].Order.ShouldBe(1);
        items[2].Order.ShouldBe(2);
        items[3].Order.ShouldBe(3);
    }

    [Fact]
    public async Task UpdateSoilFormula_MultipleUpdates_ShouldReturnOkForEach()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);

        // Create a soil formula
        var createdFormula = await CreateSoilFormula(client, soilType1, cts);

        // Act - Update multiple times
        var firstName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var firstFormulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 70, 0),
            new(soilType2.Id, 30, 1)
        };
        var firstUpdateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(firstName, firstFormulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var firstResponse = await client.PatchAsJsonAsync(url, firstUpdateRequest, cts);

        var secondName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var secondFormulaItems = new List<SoilFormulaItemDTO> { new(soilType2.Id, 100, 0) };
        var secondUpdateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(secondName, secondFormulaItems);
        var secondResponse = await client.PatchAsJsonAsync(url, secondUpdateRequest, cts);

        // Assert
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var firstUpdatedFormula = await firstResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        firstUpdatedFormula.ShouldNotBeNull();
        firstUpdatedFormula.Name.ShouldBe(firstName);
        firstUpdatedFormula.Items.Count().ShouldBe(2);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var secondUpdatedFormula = await secondResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        secondUpdatedFormula.ShouldNotBeNull();
        secondUpdatedFormula.Name.ShouldBe(secondName);
        secondUpdatedFormula.Items.Count().ShouldBe(1);
    }

    [Theory]
    [InlineData("Premium Mix")]
    [InlineData("Custom Blend 123")]
    [InlineData("Mix-A1")]
    public async Task UpdateSoilFormula_VariousValidNames_ShouldReturnOk(string namePrefix)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Create a soil formula
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act - Make the name unique by adding a GUID suffix
        var newName = $"{namePrefix}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(newName, formulaItems);
        var url = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.PatchAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updatedFormula = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        updatedFormula.ShouldNotBeNull();
        updatedFormula.Name.ShouldBe(newName);
    }

    // Helper methods
    private async Task<SoilTypeResponse> CreateSoilType(HttpClient client, CancellationToken cancellationToken)
    {
        var soilTypeName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var soilTypeRequest = new SoilTypeEndpoints.Add.AddSoilTypeRequest(soilTypeName);
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, soilTypeRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        var soilType = await response.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cancellationToken);
        return soilType!;
    }

    private async Task<SoilFormulaResponse> CreateSoilFormula(HttpClient client, SoilTypeResponse soilType, CancellationToken cancellationToken, string? name = null)
    {
        var formulaName = name ?? $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);
        var response = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
        var formula = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cancellationToken);
        return formula!;
    }
}
