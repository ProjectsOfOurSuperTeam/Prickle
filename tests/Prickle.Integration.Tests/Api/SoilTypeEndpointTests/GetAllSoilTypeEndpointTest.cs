using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Api.Endpoints.Soil.Types;
using Prickle.Application.Soil.Types;

namespace Prickle.Integration.Tests.Api.SoilTypeEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class GetAllSoilTypeEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;
    private readonly ITestOutputHelper _testOutputHelper;

    public GetAllSoilTypeEndpointTest(AppHostFixture appHostFactory, ITestOutputHelper testOutputHelper) : base(appHostFactory)
    {
        _faker = new Faker();
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task GetAllSoilTypes_NoFilters_ShouldReturnOkWithSoilTypes()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create some soil types
        var testPrefix = $"GetAll-{Guid.NewGuid():N}";
        var firstSoilTypeName = $"{testPrefix}-First";
        var secondSoilTypeName = $"{testPrefix}-Second";

        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(firstSoilTypeName), cts);
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(secondSoilTypeName), cts);

        // Act
        var response = await client.GetAsync(ApiEndpoints.Soil.Types.GetAll, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.ShouldNotBeNull();
        soilTypesResponse.Total.ShouldBeGreaterThan(0);
        soilTypesResponse.Items.Count().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllSoilTypes_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types with unique prefix
        var testPrefix = $"Pagination-{Guid.NewGuid():N}";
        for (var i = 1; i <= 5; i++)
        {
            var soilTypeName = $"{testPrefix}-Soil{i:D2}";
            await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(soilTypeName), cts);
        }

        // Act - Get first page with page size 2
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?page=1&pageSize=2", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Page.ShouldBe(1);
        soilTypesResponse.PageSize.ShouldBe(2);
        soilTypesResponse.Items.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetAllSoilTypes_WithNameFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types with unique prefix
        var uniquePrefix = $"Filter-{Guid.NewGuid():N}";
        var searchableName = $"{uniquePrefix}-SearchableType";
        var otherName = $"{uniquePrefix}-OtherType";

        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(searchableName), cts);
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(otherName), cts);

        // Act - Filter by "Searchable"
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?name=Searchable", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.ShouldNotBeEmpty();
        soilTypesResponse.Items.ShouldAllBe(st => st.Name.Contains("Searchable"));
    }

    [Fact]
    public async Task GetAllSoilTypes_WithNonExistentNameFilter_ShouldReturnEmptyList()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act - Filter by non-existent name
        var nonExistentName = $"NonExistent-{Guid.NewGuid():N}";
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?name={nonExistentName}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.ShouldBeEmpty();
        soilTypesResponse.Total.ShouldBe(0);
    }

    [Fact]
    public async Task GetAllSoilTypes_SortByNameAscending_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types with unique prefix
        var testPrefix = $"Sort-{Guid.NewGuid():N}";
        var names = new[] { "Zebra", "Alpha", "Gamma" };
        foreach (var name in names)
        {
            var soilTypeName = $"{testPrefix}-{name}";
            await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(soilTypeName), cts);
        }

        // Act - Sort by name ascending
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?sortBy=name&name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.Count().ShouldBe(3);

        // Verify ascending order
        soilTypesResponse.Items.ElementAt(0).Name.ShouldContain("Alpha");
        soilTypesResponse.Items.ElementAt(1).Name.ShouldContain("Gamma");
        soilTypesResponse.Items.ElementAt(2).Name.ShouldContain("Zebra");
    }

    [Fact]
    public async Task GetAllSoilTypes_SortByNameDescending_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types with unique prefix
        var testPrefix = $"SortDesc-{Guid.NewGuid():N}";
        var names = new[] { "Zebra", "Alpha", "Gamma" };
        foreach (var name in names)
        {
            var soilTypeName = $"{testPrefix}-{name}";
            await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(soilTypeName), cts);
        }

        // Act - Sort by name descending
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?sortBy=-name&name={testPrefix}", cts);
        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync(cts));
        // Assert

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.Count().ShouldBe(3);

        // Verify descending order
        soilTypesResponse.Items.ElementAt(0).Name.ShouldContain("Zebra");
        soilTypesResponse.Items.ElementAt(1).Name.ShouldContain("Gamma");
        soilTypesResponse.Items.ElementAt(2).Name.ShouldContain("Alpha");
    }

    [Fact]
    public async Task GetAllSoilTypes_FilterAndPagination_ShouldReturnCorrectResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types with unique prefix
        var testPrefix = $"FilterPage-{Guid.NewGuid():N}";
        for (var i = 1; i <= 5; i++)
        {
            var soilTypeName = $"{testPrefix}-Clay{i:D2}";
            await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(soilTypeName), cts);
        }

        // Act - Filter by "Clay" and get first page with size 2
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?name=Clay&page=1&pageSize=2", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.Count().ShouldBe(2);
        soilTypesResponse.Items.ShouldAllBe(st => st.Name.Contains("Clay"));
        soilTypesResponse.Total.ShouldBeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetAllSoilTypes_FilterSortAndPagination_ShouldReturnCorrectResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types with unique prefix
        var testPrefix = $"FilterSortPage-{Guid.NewGuid():N}";
        var names = new[] { "Loam", "Sand", "Clay", "Peat" };
        foreach (var name in names)
        {
            var soilTypeName = $"{testPrefix}-{name}";
            await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(soilTypeName), cts);
        }

        // Act - Filter by prefix, sort by name descending, and paginate
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?name={testPrefix}&sortBy=-name&page=1&pageSize=2", cts);
        _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync(cts));
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.Count().ShouldBe(2);
        soilTypesResponse.Items.ShouldAllBe(st => st.Name.Contains(testPrefix));

        // First item should start with 'S' (Sand), second with 'P' (Peat) in descending order
        soilTypesResponse.Items.ElementAt(0).Name.ShouldContain("Sand");
        soilTypesResponse.Items.ElementAt(1).Name.ShouldContain("Peat");
    }

    [Fact]
    public async Task GetAllSoilTypes_SecondPage_ShouldReturnCorrectItems()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types with unique prefix
        var testPrefix = $"Page2-{Guid.NewGuid():N}";
        for (var i = 1; i <= 5; i++)
        {
            var soilTypeName = $"{testPrefix}-Item{i:D2}";
            await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(soilTypeName), cts);
        }

        // Act - Get second page with page size 2
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?name={testPrefix}&page=2&pageSize=2&sortBy=name", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Page.ShouldBe(2);
        soilTypesResponse.PageSize.ShouldBe(2);
        soilTypesResponse.Items.Count().ShouldBe(2);
        soilTypesResponse.Total.ShouldBe(5);
    }

    [Fact]
    public async Task GetAllSoilTypes_DefaultPageSize_ShouldReturn10Items()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act - Request without specifying page size
        var response = await client.GetAsync(ApiEndpoints.Soil.Types.GetAll, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.PageSize.ShouldBe(10); // Default page size from PagedRequest
    }

    [Fact]
    public async Task GetAllSoilTypes_PageBeyondTotal_ShouldReturnEmptyList()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create only 2 soil types
        var testPrefix = $"BeyondPage-{Guid.NewGuid():N}";
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest($"{testPrefix}-One"), cts);
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest($"{testPrefix}-Two"), cts);

        // Act - Request page 100 (way beyond available data)
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?name={testPrefix}&page=100&pageSize=2", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.ShouldBeEmpty();
        soilTypesResponse.Total.ShouldBe(2); // Total should still reflect actual count
    }

    [Fact]
    public async Task GetAllSoilTypes_ResponseStructure_ShouldHaveCorrectProperties()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type
        var testPrefix = $"Structure-{Guid.NewGuid():N}";
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest($"{testPrefix}-Test"), cts);

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();

        // Verify response structure
        soilTypesResponse.Items.ShouldNotBeNull();
        soilTypesResponse.Total.ShouldBeGreaterThan(0);
        soilTypesResponse.Page.ShouldBeGreaterThanOrEqualTo(1);
        soilTypesResponse.PageSize.ShouldBeGreaterThan(0);

        // Verify item structure
        var firstItem = soilTypesResponse.Items.First();
        firstItem.Id.ShouldBeGreaterThan(0);
        firstItem.Name.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetAllSoilTypes_MultipleCallsWithSameParameters_ShouldReturnConsistentResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types
        var testPrefix = $"Consistent-{Guid.NewGuid():N}";
        for (var i = 1; i <= 3; i++)
        {
            await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest($"{testPrefix}-Item{i}"), cts);
        }

        // Act - Make two identical requests
        var url = $"{ApiEndpoints.Soil.Types.GetAll}?name={testPrefix}&sortBy=name";
        var firstResponse = await client.GetAsync(url, cts);
        var secondResponse = await client.GetAsync(url, cts);

        // Assert
        var firstResult = await firstResponse.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        var secondResult = await secondResponse.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);

        firstResult.ShouldNotBeNull();
        secondResult.ShouldNotBeNull();
        firstResult.Total.ShouldBe(secondResult.Total);
        firstResult.Items.Count().ShouldBe(secondResult.Items.Count());

        var firstItems = firstResult.Items.ToList();
        var secondItems = secondResult.Items.ToList();
        for (var i = 0; i < firstItems.Count; i++)
        {
            firstItems[i].Id.ShouldBe(secondItems[i].Id);
            firstItems[i].Name.ShouldBe(secondItems[i].Name);
        }
    }

    [Fact]
    public async Task GetAllSoilTypes_CaseInsensitiveFilter_ShouldFindResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a soil type with mixed case
        var testPrefix = $"CaseTest-{Guid.NewGuid():N}";
        var soilTypeName = $"{testPrefix}-MixedCaseType";
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(soilTypeName), cts);

        // Act - Search with matching case
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?name=mixedcase", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.ShouldContain(st => st.Name.Contains("MixedCase"));
    }

    [Fact]
    public async Task GetAllSoilTypes_AfterDeletion_ShouldNotIncludeDeletedItem()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create two soil types
        var testPrefix = $"Deleted-{Guid.NewGuid():N}";
        var firstSoilTypeName = $"{testPrefix}-First";
        var secondSoilTypeName = $"{testPrefix}-Second";

        var firstCreateResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(firstSoilTypeName), cts);
        var firstCreatedSoilType = await firstCreateResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);

        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(secondSoilTypeName), cts);

        // Delete the first one
        var deleteUrl = ApiEndpoints.Soil.Types.Delete.Replace("{id:int}", firstCreatedSoilType!.Id.ToString());
        await client.DeleteAsync(deleteUrl, cts);

        // Act - Get all with filter
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Total.ShouldBe(1); // Only one should remain
        soilTypesResponse.Items.Count().ShouldBe(1);
        soilTypesResponse.Items.ShouldNotContain(st => st.Id == firstCreatedSoilType.Id);
        soilTypesResponse.Items.First().Name.ShouldBe(secondSoilTypeName);
    }

    [Fact]
    public async Task GetAllSoilTypes_LargePageSize_ShouldRespectLimit()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types
        var testPrefix = $"LargePageSize-{Guid.NewGuid():N}";
        for (var i = 1; i <= 3; i++)
        {
            await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest($"{testPrefix}-Item{i}"), cts);
        }

        // Act - Request with maximum allowed page size (25)
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?name={testPrefix}&pageSize=25", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.Count().ShouldBe(3); // Should only return actual items
        soilTypesResponse.Total.ShouldBe(3);
    }

    [Fact]
    public async Task GetAllSoilTypes_PartialNameMatch_ShouldFindResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types
        var testPrefix = $"Partial-{Guid.NewGuid():N}";
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest($"{testPrefix}-SandyLoam"), cts);
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest($"{testPrefix}-ClayLoam"), cts);
        await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest($"{testPrefix}-Peat"), cts);

        // Act - Search for partial match "Loam"
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?name=Loam", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.ShouldAllBe(st => st.Name.Contains("Loam"));
        soilTypesResponse.Items.ShouldNotContain(st => st.Name.Contains("Peat"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task GetAllSoilTypes_InvalidPage_ShouldReturnBadRequest(int invalidPage)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?page={invalidPage}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(26)]
    [InlineData(1000)]
    public async Task GetAllSoilTypes_InvalidPageSize_ShouldReturnBadRequest(int invalidPageSize)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?pageSize={invalidPageSize}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllSoilTypes_InvalidSortField_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?sortBy=invalidfield", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllSoilTypes_SortById_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types with unique prefix
        var testPrefix = $"SortById-{Guid.NewGuid():N}";
        var createdIds = new List<int>();

        for (var i = 1; i <= 3; i++)
        {
            var soilTypeName = $"{testPrefix}-Item{i}";
            var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(soilTypeName), cts);
            var created = await createResponse.Content.ReadFromJsonAsync<SoilTypeResponse>(cancellationToken: cts);
            createdIds.Add(created!.Id);
        }

        // Act - Sort by id ascending
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?sortBy=id&name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.Count().ShouldBe(3);

        // Verify ascending order by ID
        var items = soilTypesResponse.Items.ToList();
        for (var i = 0; i < items.Count - 1; i++)
        {
            items[i].Id.ShouldBeLessThan(items[i + 1].Id);
        }
    }

    [Fact]
    public async Task GetAllSoilTypes_SortByIdDescending_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create soil types with unique prefix
        var testPrefix = $"SortByIdDesc-{Guid.NewGuid():N}";

        for (var i = 1; i <= 3; i++)
        {
            var soilTypeName = $"{testPrefix}-Item{i}";
            await client.PostAsJsonAsync(ApiEndpoints.Soil.Types.Add, new Add.AddSoilTypeRequest(soilTypeName), cts);
        }

        // Act - Sort by id descending
        var response = await client.GetAsync($"{ApiEndpoints.Soil.Types.GetAll}?sortBy=-id&name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var soilTypesResponse = await response.Content.ReadFromJsonAsync<SoilTypesResponse>(cancellationToken: cts);
        soilTypesResponse.ShouldNotBeNull();
        soilTypesResponse.Items.Count().ShouldBe(3);

        // Verify descending order by ID
        var items = soilTypesResponse.Items.ToList();
        for (var i = 0; i < items.Count - 1; i++)
        {
            items[i].Id.ShouldBeGreaterThan(items[i + 1].Id);
        }
    }
}
