using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Soil.Formulas;
using Prickle.Application.Soil.Types;
using SoilFormulaEndpoints = Prickle.Api.Endpoints.Soil.Formulas;
using SoilTypeEndpoints = Prickle.Api.Endpoints.Soil.Types;

namespace Prickle.Integration.Tests.Api.SoilFormulaEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class GetSoilFormulaEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public GetSoilFormulaEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task GetSoilFormula_ExistingId_ShouldReturnOkWithSoilFormula()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types and formula
        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        
        var formulaName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 60, 0),
            new(soilType2.Id, 40, 1)
        };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        var createdFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", createdFormula!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();
        soilFormulaResponse.Id.ShouldBe(createdFormula.Id);
        soilFormulaResponse.Name.ShouldBe(formulaName);
        soilFormulaResponse.Items.Count().ShouldBe(2);

        var items = soilFormulaResponse.Items.OrderBy(i => i.Order).ToList();
        items[0].SoilType.Id.ShouldBe(soilType1.Id);
        items[0].Percentage.ShouldBe(60);
        items[0].Order.ShouldBe(0);
        items[1].SoilType.Id.ShouldBe(soilType2.Id);
        items[1].Percentage.ShouldBe(40);
        items[1].Order.ShouldBe(1);
    }

    [Fact]
    public async Task GetSoilFormula_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");
        var nonExistentId = Guid.NewGuid();

        // Act
        var url = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", nonExistentId.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSoilFormula_EmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var url = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", Guid.Empty.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSoilFormula_MultipleRequests_ShouldReturnConsistentResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act - Make multiple GET requests
        var url = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", createdFormula.Id.ToString());
        var firstResponse = await client.GetAsync(url, cts);
        var secondResponse = await client.GetAsync(url, cts);

        // Assert
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var firstFormula = await firstResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        var secondFormula = await secondResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        firstFormula.ShouldNotBeNull();
        secondFormula.ShouldNotBeNull();
        firstFormula.Id.ShouldBe(secondFormula.Id);
        firstFormula.Name.ShouldBe(secondFormula.Name);
        firstFormula.Items.Count().ShouldBe(secondFormula.Items.Count());
    }

    [Fact]
    public async Task GetSoilFormula_AfterUpdate_ShouldReturnUpdatedData()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);

        // Create formula
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType1.Id, 100, 0) };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(originalName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        var createdFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        // Update formula
        var updatedName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var updatedItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 60, 0),
            new(soilType2.Id, 40, 1)
        };
        var updateRequest = new SoilFormulaEndpoints.Update.UpdateSoilFormulaRequest(updatedName, updatedItems);
        var updateUrl = ApiEndpoints.Soil.Formulas.Update.Replace("{id:guid}", createdFormula!.Id.ToString());
        await client.PatchAsJsonAsync(updateUrl, updateRequest, cts);

        // Act - Get the updated formula
        var getUrl = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", createdFormula.Id.ToString());
        var getResponse = await client.GetAsync(getUrl, cts);

        // Assert
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var retrievedFormula = await getResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        retrievedFormula.ShouldNotBeNull();
        retrievedFormula.Name.ShouldBe(updatedName);
        retrievedFormula.Items.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetSoilFormula_WithSingleItem_ShouldReturnCorrectly()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act
        var url = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();
        soilFormulaResponse.Items.Count().ShouldBe(1);
        soilFormulaResponse.Items.First().Percentage.ShouldBe(100);
    }

    [Fact]
    public async Task GetSoilFormula_WithMultipleItems_ShouldReturnAllItems()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);

        var formulaName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 50, 0),
            new(soilType2.Id, 30, 1),
            new(soilType3.Id, 20, 2)
        };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        var createdFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", createdFormula!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();
        soilFormulaResponse.Items.Count().ShouldBe(3);

        var items = soilFormulaResponse.Items.OrderBy(i => i.Order).ToList();
        items[0].Percentage.ShouldBe(50);
        items[1].Percentage.ShouldBe(30);
        items[2].Percentage.ShouldBe(20);
    }

    [Fact]
    public async Task GetSoilFormula_VerifyItemsHaveSoilTypeDetails()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);

        var formulaName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 70, 0),
            new(soilType2.Id, 30, 1)
        };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        var createdFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", createdFormula!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();

        var items = soilFormulaResponse.Items.OrderBy(i => i.Order).ToList();
        items[0].SoilType.ShouldNotBeNull();
        items[0].SoilType.Id.ShouldBe(soilType1.Id);
        items[0].SoilType.Name.ShouldBe(soilType1.Name);
        items[1].SoilType.ShouldNotBeNull();
        items[1].SoilType.Id.ShouldBe(soilType2.Id);
        items[1].SoilType.Name.ShouldBe(soilType2.Name);
    }

    [Fact]
    public async Task GetSoilFormula_ComplexFormula_ShouldReturnCompleteData()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);
        var soilType4 = await CreateSoilType(client, cts);

        var formulaName = $"Complex-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 40, 0),
            new(soilType2.Id, 25, 1),
            new(soilType3.Id, 20, 2),
            new(soilType4.Id, 15, 3)
        };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createdFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", createdFormula!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();
        soilFormulaResponse.Items.Count().ShouldBe(4);
        
        var items = soilFormulaResponse.Items.OrderBy(i => i.Order).ToList();
        items[0].Order.ShouldBe(0);
        items[1].Order.ShouldBe(1);
        items[2].Order.ShouldBe(2);
        items[3].Order.ShouldBe(3);

        // Verify all percentages
        items.Sum(i => i.Percentage).ShouldBe(100);
    }

    [Fact]
    public async Task GetSoilFormula_AfterCreation_ShouldReturnImmediately()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);

        // Act - Create and immediately retrieve
        var formulaName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var formulaItems = new List<SoilFormulaItemDTO> { new(soilType.Id, 100, 0) };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);
        var createdFormula = await createResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        var url = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", createdFormula!.Id.ToString());
        var getResponse = await client.GetAsync(url, cts);

        // Assert
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var retrievedFormula = await getResponse.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        retrievedFormula.ShouldNotBeNull();
        retrievedFormula.Id.ShouldBe(createdFormula.Id);
        retrievedFormula.Name.ShouldBe(formulaName);
    }

    [Theory]
    [InlineData("Premium Mix")]
    [InlineData("Custom Blend")]
    [InlineData("Test Formula 123")]
    public async Task GetSoilFormula_VariousNames_ShouldReturnCorrectFormula(string namePrefix)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var formulaName = $"{namePrefix}-{Guid.NewGuid():N}";
        var createdFormula = await CreateSoilFormula(client, soilType, cts, formulaName);

        // Act
        var url = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", createdFormula.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulaResponse = await response.Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        soilFormulaResponse.ShouldNotBeNull();
        soilFormulaResponse.Name.ShouldBe(formulaName);
    }

    [Fact]
    public async Task GetSoilFormula_ConcurrentGets_ShouldAllSucceed()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var soilType = await CreateSoilType(client, cts);
        var createdFormula = await CreateSoilFormula(client, soilType, cts);

        // Act - Make concurrent GET requests
        var url = ApiEndpoints.Soil.Formulas.Get.Replace("{id:guid}", createdFormula.Id.ToString());
        
        var task1 = client.GetAsync(url, cts);
        var task2 = client.GetAsync(url, cts);
        var task3 = client.GetAsync(url, cts);

        var responses = await Task.WhenAll(task1, task2, task3);

        // Assert
        responses[0].StatusCode.ShouldBe(HttpStatusCode.OK);
        responses[1].StatusCode.ShouldBe(HttpStatusCode.OK);
        responses[2].StatusCode.ShouldBe(HttpStatusCode.OK);

        var formula1 = await responses[0].Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        var formula2 = await responses[1].Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);
        var formula3 = await responses[2].Content.ReadFromJsonAsync<SoilFormulaResponse>(cancellationToken: cts);

        formula1.ShouldNotBeNull();
        formula2.ShouldNotBeNull();
        formula3.ShouldNotBeNull();
        formula1.Id.ShouldBe(formula2.Id);
        formula2.Id.ShouldBe(formula3.Id);
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
