using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Soil.Formulas;
using Prickle.Application.Soil.Types;
using SoilFormulaEndpoints = Prickle.Api.Endpoints.Soil.Formulas;
using SoilTypeEndpoints = Prickle.Api.Endpoints.Soil.Types;

namespace Prickle.Integration.Tests.Api.SoilFormulaEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class GetAllSoilFormulasEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;
    private readonly ITestOutputHelper _testOutputHelper;

    public GetAllSoilFormulasEndpointTest(AppHostFixture appHostFactory, ITestOutputHelper testOutputHelper) : base(appHostFactory)
    {
        _faker = new Faker();
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task GetAllSoilFormulas_NoFilters_ShouldReturnOkWithSoilFormulas()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create some soil formulas
        var testPrefix = $"GetAll-{Guid.NewGuid():N}";
        var soilType = await CreateSoilType(client, cts);
        
        await CreateSoilFormula(client, soilType, cts, $"{testPrefix}-First");
        await CreateSoilFormula(client, soilType, cts, $"{testPrefix}-Second");

        // Act
        var response = await client.GetAsync(ApiEndpoints.Soil.Formulas.GetAll, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.ShouldNotBeNull();
        soilFormulasResponse.Total.ShouldBeGreaterThan(0);
        soilFormulasResponse.Items.Count().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllSoilFormulas_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil formulas with unique prefix
        var testPrefix = $"Pagination-{Guid.NewGuid():N}";
        var soilType = await CreateSoilType(client, cts);
        
        for (var i = 1; i <= 5; i++)
        {
            var formulaName = $"{testPrefix}-Formula{i:D2}";
            await CreateSoilFormula(client, soilType, cts, formulaName);
        }

        // Act - Get first page with page size 2
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?page=1&pageSize=2", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Page.ShouldBe(1);
        soilFormulasResponse.PageSize.ShouldBe(2);
        soilFormulasResponse.Items.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetAllSoilFormulas_WithNameFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil formulas with unique prefix
        var uniquePrefix = $"Filter-{Guid.NewGuid():N}";
        var soilType = await CreateSoilType(client, cts);
        
        var searchableName = $"{uniquePrefix}-SearchableFormula";
        var otherName = $"{uniquePrefix}-OtherFormula";

        await CreateSoilFormula(client, soilType, cts, searchableName);
        await CreateSoilFormula(client, soilType, cts, otherName);

        // Act - Filter by "Searchable"
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?name=Searchable", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.ShouldNotBeEmpty();
        soilFormulasResponse.Items.ShouldAllBe(sf => sf.Name.Contains("Searchable"));
    }

    [Fact]
    public async Task GetAllSoilFormulas_WithNonExistentNameFilter_ShouldReturnEmptyList()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act - Filter by non-existent name
        var nonExistentName = $"NonExistent-{Guid.NewGuid():N}";
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?name={nonExistentName}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.ShouldBeEmpty();
        soilFormulasResponse.Total.ShouldBe(0);
    }

    [Fact]
    public async Task GetAllSoilFormulas_SortByNameAscending_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil formulas with unique prefix
        var testPrefix = $"Sort-{Guid.NewGuid():N}";
        var soilType = await CreateSoilType(client, cts);
        
        var names = new[] { "Zebra", "Alpha", "Gamma" };
        foreach (var name in names)
        {
            var formulaName = $"{testPrefix}-{name}";
            await CreateSoilFormula(client, soilType, cts, formulaName);
        }

        // Act - Sort by name ascending
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?sortBy=name&name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.Count().ShouldBe(3);

        // Verify ascending order
        soilFormulasResponse.Items.ElementAt(0).Name.ShouldContain("Alpha");
        soilFormulasResponse.Items.ElementAt(1).Name.ShouldContain("Gamma");
        soilFormulasResponse.Items.ElementAt(2).Name.ShouldContain("Zebra");
    }

    [Fact]
    public async Task GetAllSoilFormulas_SortByNameDescending_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil formulas with unique prefix
        var testPrefix = $"SortDesc-{Guid.NewGuid():N}";
        var soilType = await CreateSoilType(client, cts);
        
        var names = new[] { "Zebra", "Alpha", "Gamma" };
        foreach (var name in names)
        {
            var formulaName = $"{testPrefix}-{name}";
            await CreateSoilFormula(client, soilType, cts, formulaName);
        }

        // Act - Sort by name descending
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?sortBy=-name&name={testPrefix}", cts);
        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync(cts));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.Count().ShouldBe(3);

        // Verify descending order
        soilFormulasResponse.Items.ElementAt(0).Name.ShouldContain("Zebra");
        soilFormulasResponse.Items.ElementAt(1).Name.ShouldContain("Gamma");
        soilFormulasResponse.Items.ElementAt(2).Name.ShouldContain("Alpha");
    }

    [Fact]
    public async Task GetAllSoilFormulas_FilterAndPagination_ShouldReturnCorrectResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil formulas with unique prefix
        var testPrefix = $"FilterPage-{Guid.NewGuid():N}";
        var soilType = await CreateSoilType(client, cts);
        
        for (var i = 1; i <= 5; i++)
        {
            var formulaName = $"{testPrefix}-Formula{i:D2}";
            await CreateSoilFormula(client, soilType, cts, formulaName);
        }

        // Act - Filter and paginate
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?name={testPrefix}&page=1&pageSize=2", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.Count().ShouldBe(2);
        soilFormulasResponse.Total.ShouldBeGreaterThanOrEqualTo(5);
        soilFormulasResponse.Items.ShouldAllBe(sf => sf.Name.Contains(testPrefix));
    }

    [Fact]
    public async Task GetAllSoilFormulas_WithComplexFormulas_ShouldIncludeAllFormulaItems()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var testPrefix = $"Complex-{Guid.NewGuid():N}";
        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);

        // Create a complex formula
        var formulaName = $"{testPrefix}-ComplexFormula";
        var formulaItems = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 50, 0),
            new(soilType2.Id, 30, 1),
            new(soilType3.Id, 20, 2)
        };
        var createRequest = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formulaName, formulaItems);
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, createRequest, cts);

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.Count().ShouldBe(1);

        var formula = soilFormulasResponse.Items.First();
        formula.Items.Count().ShouldBe(3);
        formula.Items.Sum(i => i.Percentage).ShouldBe(100);
    }

    [Fact]
    public async Task GetAllSoilFormulas_MultiplePages_ShouldPaginateCorrectly()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var testPrefix = $"Pages-{Guid.NewGuid():N}";
        var soilType = await CreateSoilType(client, cts);

        // Create 7 formulas
        for (var i = 1; i <= 7; i++)
        {
            var formulaName = $"{testPrefix}-Formula{i:D2}";
            await CreateSoilFormula(client, soilType, cts, formulaName);
        }

        // Act - Get page 1 (3 items)
        var page1Response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?name={testPrefix}&page=1&pageSize=3", cts);
        var page1 = await page1Response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);

        // Act - Get page 2 (3 items)
        var page2Response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?name={testPrefix}&page=2&pageSize=3", cts);
        var page2 = await page2Response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);

        // Act - Get page 3 (1 item)
        var page3Response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?name={testPrefix}&page=3&pageSize=3", cts);
        var page3 = await page3Response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);

        // Assert
        page1.ShouldNotBeNull();
        page1.Items.Count().ShouldBe(3);
        page1.Total.ShouldBe(7);

        page2.ShouldNotBeNull();
        page2.Items.Count().ShouldBe(3);
        page2.Total.ShouldBe(7);

        page3.ShouldNotBeNull();
        page3.Items.Count().ShouldBe(1);
        page3.Total.ShouldBe(7);

        // Verify no duplicates
        var allIds = page1.Items.Concat(page2.Items).Concat(page3.Items).Select(f => f.Id).ToList();
        allIds.Distinct().Count().ShouldBe(7);
    }

    [Fact]
    public async Task GetAllSoilFormulas_InvalidPageSize_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act - Request with invalid page size (26 > 25 max)
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?pageSize=26", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllSoilFormulas_InvalidSortField_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act - Request with invalid sort field
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?sortBy=invalid", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllSoilFormulas_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act - Search for non-existent formulas
        var uniqueName = $"Empty-{Guid.NewGuid():N}";
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?name={uniqueName}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.ShouldBeEmpty();
        soilFormulasResponse.Total.ShouldBe(0);
    }

    [Fact]
    public async Task GetAllSoilFormulas_DefaultPaging_ShouldUseDefaults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act - No paging parameters
        var response = await client.GetAsync(ApiEndpoints.Soil.Formulas.GetAll, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Page.ShouldBeGreaterThan(0);
        soilFormulasResponse.PageSize.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("Premium")]
    [InlineData("Custom")]
    [InlineData("Special")]
    public async Task GetAllSoilFormulas_PartialNameMatch_ShouldReturnMatchingResults(string searchTerm)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var testId = Guid.NewGuid().ToString("N");
        var soilType = await CreateSoilType(client, cts);
        
        // Create formula with search term
        var formulaName = $"{searchTerm}-Formula-{testId}";
        await CreateSoilFormula(client, soilType, cts, formulaName);

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?name={searchTerm}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.ShouldNotBeEmpty();
        soilFormulasResponse.Items.ShouldAllBe(sf => sf.Name.Contains(searchTerm));
    }

    [Fact]
    public async Task GetAllSoilFormulas_FilterBySingleSoilType_ShouldReturnOnlyFormulasWithThatType()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var testPrefix = $"SoilTypeFilter-{Guid.NewGuid():N}";
        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);

        // Create formulas with different soil types
        var formula1 = await CreateSoilFormula(client, soilType1, cts, $"{testPrefix}-WithType1");
        var formula2Name = $"{testPrefix}-WithType2";
        var formula2Items = new List<SoilFormulaItemDTO> { new(soilType2.Id, 100, 0) };
        var formula2Request = new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formula2Name, formula2Items);
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add, formula2Request, cts);

        // Act - Filter by soilType1
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?soilTypeIds={soilType1.Id}&name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.Count().ShouldBe(1);
        soilFormulasResponse.Items.First().Name.ShouldBe($"{testPrefix}-WithType1");
        soilFormulasResponse.Items.First().Items.ShouldContain(i => i.SoilType.Id == soilType1.Id);
    }

    [Fact]
    public async Task GetAllSoilFormulas_FilterByMultipleSoilTypes_ShouldReturnOnlyFormulasWithAllTypes()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var testPrefix = $"MultiFilter-{Guid.NewGuid():N}";
        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);

        // Create formula with soilType1 and soilType2
        var formula1Name = $"{testPrefix}-With1And2";
        var formula1Items = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 60, 0),
            new(soilType2.Id, 40, 1)
        };
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add,
            new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formula1Name, formula1Items), cts);

        // Create formula with only soilType1
        await CreateSoilFormula(client, soilType1, cts, $"{testPrefix}-OnlyType1");

        // Create formula with soilType1, soilType2, and soilType3
        var formula3Name = $"{testPrefix}-With1And2And3";
        var formula3Items = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 40, 0),
            new(soilType2.Id, 30, 1),
            new(soilType3.Id, 30, 2)
        };
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add,
            new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formula3Name, formula3Items), cts);

        // Act - Filter by soilType1 AND soilType2 (formulas must contain BOTH)
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?soilTypeIds={soilType1.Id}&soilTypeIds={soilType2.Id}&name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.Count().ShouldBe(2); // formula1 and formula3

        // Verify all returned formulas contain both soil types
        foreach (var formula in soilFormulasResponse.Items)
        {
            formula.Items.Select(i => i.SoilType.Id).ShouldContain(soilType1.Id);
            formula.Items.Select(i => i.SoilType.Id).ShouldContain(soilType2.Id);
        }
    }

    [Fact]
    public async Task GetAllSoilFormulas_SortByItemCountAscending_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var testPrefix = $"ItemSort-{Guid.NewGuid():N}";
        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);

        // Create formula with 1 item
        await CreateSoilFormula(client, soilType1, cts, $"{testPrefix}-OneItem");

        // Create formula with 3 items
        var formula3Name = $"{testPrefix}-ThreeItems";
        var formula3Items = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 50, 0),
            new(soilType2.Id, 30, 1),
            new(soilType3.Id, 20, 2)
        };
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add,
            new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formula3Name, formula3Items), cts);

        // Create formula with 2 items
        var formula2Name = $"{testPrefix}-TwoItems";
        var formula2Items = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 60, 0),
            new(soilType2.Id, 40, 1)
        };
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add,
            new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formula2Name, formula2Items), cts);

        // Act - Sort by itemcount ascending
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?sortBy=itemcount&name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.Count().ShouldBe(3);

        // Verify ascending order by item count
        soilFormulasResponse.Items.ElementAt(0).Items.Count().ShouldBe(1);
        soilFormulasResponse.Items.ElementAt(1).Items.Count().ShouldBe(2);
        soilFormulasResponse.Items.ElementAt(2).Items.Count().ShouldBe(3);
    }

    [Fact]
    public async Task GetAllSoilFormulas_SortByItemCountDescending_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var testPrefix = $"ItemSortDesc-{Guid.NewGuid():N}";
        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);
        var soilType3 = await CreateSoilType(client, cts);

        // Create formula with 1 item
        await CreateSoilFormula(client, soilType1, cts, $"{testPrefix}-OneItem");

        // Create formula with 3 items
        var formula3Name = $"{testPrefix}-ThreeItems";
        var formula3Items = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 50, 0),
            new(soilType2.Id, 30, 1),
            new(soilType3.Id, 20, 2)
        };
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add,
            new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formula3Name, formula3Items), cts);

        // Create formula with 2 items
        var formula2Name = $"{testPrefix}-TwoItems";
        var formula2Items = new List<SoilFormulaItemDTO>
        {
            new(soilType1.Id, 60, 0),
            new(soilType2.Id, 40, 1)
        };
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add,
            new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(formula2Name, formula2Items), cts);

        // Act - Sort by itemcount descending
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?sortBy=-itemcount&name={testPrefix}", cts);
        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync(cts));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.Count().ShouldBe(3);

        // Verify descending order by item count
        soilFormulasResponse.Items.ElementAt(0).Items.Count().ShouldBe(3);
        soilFormulasResponse.Items.ElementAt(1).Items.Count().ShouldBe(2);
        soilFormulasResponse.Items.ElementAt(2).Items.Count().ShouldBe(1);
    }

    [Fact]
    public async Task GetAllSoilFormulas_FilterBySoilTypeWithInvalidId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act - Filter by invalid soil type ID (0)
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?soilTypeIds=0", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllSoilFormulas_FilterByTooManySoilTypes_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act - Filter by more than 10 soil type IDs
        var soilTypeIds = string.Join("&", Enumerable.Range(1, 11).Select(i => $"soilTypeIds={i}"));
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?{soilTypeIds}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllSoilFormulas_CombineNameAndSoilTypeFilter_ShouldReturnMatchingResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var testPrefix = $"Combined-{Guid.NewGuid():N}";
        var soilType1 = await CreateSoilType(client, cts);
        var soilType2 = await CreateSoilType(client, cts);

        // Create formula with name "Premium" and soilType1
        var premiumName = $"{testPrefix}-Premium";
        await CreateSoilFormula(client, soilType1, cts, premiumName);

        // Create formula with name "Standard" and soilType2
        var standardName = $"{testPrefix}-Standard";
        var standardItems = new List<SoilFormulaItemDTO> { new(soilType2.Id, 100, 0) };
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Formulas.Add,
            new SoilFormulaEndpoints.Add.AddSoilFormulaRequest(standardName, standardItems), cts);

        // Act - Filter by name "Premium" AND soilType1
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Formulas.GetAll}?name=Premium&soilTypeIds={soilType1.Id}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilFormulasResponse = await response.Content.ReadFromJsonAsync<SoilFormulasResponse>(cancellationToken: cts);
        soilFormulasResponse.ShouldNotBeNull();
        soilFormulasResponse.Items.ShouldNotBeEmpty();
        soilFormulasResponse.Items.ShouldAllBe(sf => sf.Name.Contains("Premium"));
        soilFormulasResponse.Items.ShouldAllBe(sf => sf.Items.Any(i => i.SoilType.Id == soilType1.Id));
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
