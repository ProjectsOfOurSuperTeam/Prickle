using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Decorations;
using Prickle.Domain.Decorations;
using SharedKernel;
using DecorationsEndpoints = Prickle.Api.Endpoints.Decorations;

namespace Prickle.Integration.Tests.Api.DecorationsEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class GetAllDecorationEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;
    private readonly ITestOutputHelper _testOutputHelper;

    public GetAllDecorationEndpointTest(AppHostFixture appHostFactory, ITestOutputHelper testOutputHelper) : base(appHostFactory)
    {
        _faker = new Faker();
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task GetAllDecorations_NoFilters_ShouldReturnOkWithDecorations()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create some decorations
        var testPrefix = $"GetAll-{Guid.NewGuid():N}";
        var firstDecorationName = $"{testPrefix}-First";
        var secondDecorationName = $"{testPrefix}-Second";

        await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add,
            new DecorationsEndpoints.Add.AddDecorationRequest { Name = firstDecorationName, Category = DecorationCategory.Stones.Value }, cts);
        await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add,
            new DecorationsEndpoints.Add.AddDecorationRequest { Name = secondDecorationName, Category = DecorationCategory.Sand.Value }, cts);

        // Act
        var response = await client.GetAsync(ApiEndpoints.Decorations.GetAll, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationsResponse = await response.Content.ReadFromJsonAsync<DecorationsResponse>(cancellationToken: cts);
        decorationsResponse.ShouldNotBeNull();
        decorationsResponse.Items.ShouldNotBeNull();
        decorationsResponse.Total.ShouldBeGreaterThan(0);
        decorationsResponse.Items.Count().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllDecorations_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create multiple decorations
        var testPrefix = $"Pagination-{Guid.NewGuid():N}";
        for (var i = 1; i <= 5; i++)
        {
            await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add,
                new DecorationsEndpoints.Add.AddDecorationRequest
                {
                    Name = $"{testPrefix}-Decoration-{i:00}",
                    Category = DecorationCategory.Figurines.Value
                }, cts);
        }

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Decorations.GetAll}?page=1&pageSize=3", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationsResponse = await response.Content.ReadFromJsonAsync<DecorationsResponse>(cancellationToken: cts);
        decorationsResponse.ShouldNotBeNull();
        decorationsResponse.Page.ShouldBe(1);
        decorationsResponse.PageSize.ShouldBe(3);
        decorationsResponse.Items.Count().ShouldBeLessThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllDecorations_WithNameFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var uniquePrefix = $"Filter-{Guid.NewGuid():N}";
        var matchingName = $"{uniquePrefix}-Match";
        var nonMatchingName = $"Other-{Guid.NewGuid():N}";

        await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add,
            new DecorationsEndpoints.Add.AddDecorationRequest { Name = matchingName, Category = DecorationCategory.Stones.Value }, cts);
        await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add,
            new DecorationsEndpoints.Add.AddDecorationRequest { Name = nonMatchingName, Category = DecorationCategory.Sand.Value }, cts);

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Decorations.GetAll}?name={uniquePrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationsResponse = await response.Content.ReadFromJsonAsync<DecorationsResponse>(cancellationToken: cts);
        decorationsResponse.ShouldNotBeNull();
        decorationsResponse.Items.ShouldNotBeEmpty();
        decorationsResponse.Items.ShouldAllBe(d => d.Name.Contains(uniquePrefix, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAllDecorations_WithSortingAscending_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var testPrefix = $"Sort-{Guid.NewGuid():N}";
        var firstDecoration = $"{testPrefix}-AAA";
        var secondDecoration = $"{testPrefix}-ZZZ";

        await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add,
            new DecorationsEndpoints.Add.AddDecorationRequest { Name = secondDecoration, Category = DecorationCategory.Stones.Value }, cts);
        await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add,
            new DecorationsEndpoints.Add.AddDecorationRequest { Name = firstDecoration, Category = DecorationCategory.Sand.Value }, cts);

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Decorations.GetAll}?sortBy=name&name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationsResponse = await response.Content.ReadFromJsonAsync<DecorationsResponse>(cancellationToken: cts);
        decorationsResponse.ShouldNotBeNull();
        decorationsResponse.Items.ShouldNotBeEmpty();

        var sortedItems = decorationsResponse.Items.Where(d => d.Name.Contains(testPrefix)).ToList();
        sortedItems.Count.ShouldBeGreaterThanOrEqualTo(2);

        for (var i = 0; i < sortedItems.Count - 1; i++)
        {
            string.Compare(sortedItems[i].Name, sortedItems[i + 1].Name, StringComparison.OrdinalIgnoreCase)
                .ShouldBeLessThanOrEqualTo(0);
        }
    }

    [Fact]
    public async Task GetAllDecorations_WithSortingDescending_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var testPrefix = $"SortDesc-{Guid.NewGuid():N}";
        var firstDecoration = $"{testPrefix}-AAA";
        var secondDecoration = $"{testPrefix}-ZZZ";

        await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add,
            new DecorationsEndpoints.Add.AddDecorationRequest { Name = firstDecoration, Category = DecorationCategory.Stones.Value }, cts);
        await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add,
            new DecorationsEndpoints.Add.AddDecorationRequest { Name = secondDecoration, Category = DecorationCategory.Sand.Value }, cts);

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Decorations.GetAll}?sortBy=-name&name={testPrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationsResponse = await response.Content.ReadFromJsonAsync<DecorationsResponse>(cancellationToken: cts);
        decorationsResponse.ShouldNotBeNull();
        decorationsResponse.Items.ShouldNotBeEmpty();

        var sortedItems = decorationsResponse.Items.Where(d => d.Name.Contains(testPrefix)).ToList();
        sortedItems.Count.ShouldBeGreaterThanOrEqualTo(2);

        for (var i = 0; i < sortedItems.Count - 1; i++)
        {
            string.Compare(sortedItems[i].Name, sortedItems[i + 1].Name, StringComparison.OrdinalIgnoreCase)
                .ShouldBeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public async Task GetAllDecorations_WithCombinedFilters_ShouldReturnFilteredAndSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var testPrefix = $"Combined-{Guid.NewGuid():N}";
        var decorations = new[]
        {
            $"{testPrefix}-Charlie",
            $"{testPrefix}-Alpha",
            $"{testPrefix}-Bravo"
        };

        foreach (var decorationName in decorations)
        {
            await client.PostAsJsonAsync(ApiEndpoints.Decorations.Add,
                new DecorationsEndpoints.Add.AddDecorationRequest { Name = decorationName, Category = DecorationCategory.Figurines.Value }, cts);
        }

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Decorations.GetAll}?name={testPrefix}&sortBy=name&page=1&pageSize=10", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationsResponse = await response.Content.ReadFromJsonAsync<DecorationsResponse>(cancellationToken: cts);
        decorationsResponse.ShouldNotBeNull();
        decorationsResponse.Items.ShouldNotBeEmpty();
        decorationsResponse.Items.ShouldAllBe(d => d.Name.Contains(testPrefix, StringComparison.OrdinalIgnoreCase));
        decorationsResponse.Page.ShouldBe(1);
        decorationsResponse.PageSize.ShouldBe(10);
    }

    [Fact]
    public async Task GetAllDecorations_EmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var nonExistentFilter = $"NonExistent-{Guid.NewGuid()}";

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Decorations.GetAll}?name={nonExistentFilter}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationsResponse = await response.Content.ReadFromJsonAsync<DecorationsResponse>(cancellationToken: cts);
        decorationsResponse.ShouldNotBeNull();
        decorationsResponse.Items.ShouldNotBeNull();
        decorationsResponse.Items.ShouldBeEmpty();
        decorationsResponse.Total.ShouldBe(0);
    }

    [Fact]
    public async Task GetAllDecorations_DefaultPagination_ShouldUseDefaultValues()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync(ApiEndpoints.Decorations.GetAll, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationsResponse = await response.Content.ReadFromJsonAsync<DecorationsResponse>(cancellationToken: cts);
        decorationsResponse.ShouldNotBeNull();
        decorationsResponse.Page.ShouldBe(PagedRequest.DefaultPage); // Default page
        decorationsResponse.PageSize.ShouldBe(PagedRequest.DefaultPageSize); // Default page size
    }

    [Fact]
    public async Task GetAllDecorations_ReturnsCorrectContentType()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync(ApiEndpoints.Decorations.GetAll, cts);

        // Assert
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    [Theory]
    [InlineData("?page=2&pageSize=5")]
    [InlineData("?sortBy=name")]
    [InlineData("?sortBy=-name")]
    [InlineData("?name=test")]
    [InlineData("?name=test&sortBy=name&page=1&pageSize=10")]
    public async Task GetAllDecorations_VariousQueryParameters_ShouldReturnOk(string queryParams)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Decorations.GetAll}{queryParams}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var decorationsResponse = await response.Content.ReadFromJsonAsync<DecorationsResponse>(cancellationToken: cts);
        decorationsResponse.ShouldNotBeNull();
    }
}