using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Containers;
using SharedKernel;
using ContainersEndpoints = Prickle.Api.Endpoints.Containers;

namespace Prickle.Integration.Tests.Api.ContainersEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class GetAllContainersEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public GetAllContainersEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task GetAllContainers_WithoutParameters_ShouldReturnOkWithContainers()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a few containers first
        for (var i = 0; i < 3; i++)
        {
            var createRequest = new ContainersEndpoints.Add.AddContainerRequest
            {
                Name = $"TestContainer-{i}-{Guid.NewGuid():N}",
                Description = $"Test container {i}",
                Volume = _faker.Random.Float(1.0f, 100.0f),
                IsClosed = i % 2 == 0,
                ImageUrl = $"https://example.com/image{i}.jpg",
                ImageIsometricUrl = $"https://example.com/isometric{i}.jpg"
            };
            await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        }

        // Act
        var response = await client.GetAsync(ApiEndpoints.Containers.GetAll, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containersResponse = await response.Content.ReadFromJsonAsync<ContainersResponse>(cancellationToken: cts);
        containersResponse.ShouldNotBeNull();
        containersResponse.Items.Count().ShouldBeGreaterThanOrEqualTo(3);
        containersResponse.Total.ShouldBeGreaterThanOrEqualTo(3);
        containersResponse.Page.ShouldBe(PagedRequest.DefaultPage);
        containersResponse.PageSize.ShouldBe(PagedRequest.DefaultPageSize);
    }

    [Fact]
    public async Task GetAllContainers_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create several containers
        for (var i = 0; i < 5; i++)
        {
            var createRequest = new ContainersEndpoints.Add.AddContainerRequest
            {
                Name = $"PaginationTest-{i}-{Guid.NewGuid():N}",
                Description = $"Pagination test container {i}",
                Volume = _faker.Random.Float(1.0f, 100.0f),
                IsClosed = false,
                ImageUrl = "https://example.com/image.jpg",
                ImageIsometricUrl = "https://example.com/isometric.jpg"
            };
            await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        }

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?page=1&pageSize=2", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containersResponse = await response.Content.ReadFromJsonAsync<ContainersResponse>(cancellationToken: cts);
        containersResponse.ShouldNotBeNull();
        containersResponse.Items.Count().ShouldBeLessThanOrEqualTo(2);
        containersResponse.Page.ShouldBe(1);
        containersResponse.PageSize.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllContainers_WithNameFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var uniqueName = $"FilterTest-{Guid.NewGuid():N}";
        var createRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = uniqueName,
            Description = "Filter test container",
            Volume = 50.0f,
            IsClosed = false,
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };
        await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?name={uniqueName}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containersResponse = await response.Content.ReadFromJsonAsync<ContainersResponse>(cancellationToken: cts);
        containersResponse.ShouldNotBeNull();
        containersResponse.Items.ShouldContain(c => c.Name == uniqueName);
    }

    [Fact]
    public async Task GetAllContainers_WithSortByName_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var uniquePrefix = $"Sort-{Guid.NewGuid():N}";
        var containerNames = new List<string>();
        for (var i = 0; i < 3; i++)
        {
            var name = $"{uniquePrefix}-{(char)('C' - i)}"; // Creates names like Sort-{guid}-C, Sort-{guid}-B, Sort-{guid}-A
            containerNames.Add(name);
            var createRequest = new ContainersEndpoints.Add.AddContainerRequest
            {
                Name = name,
                Description = $"Sort test container {i}",
                Volume = 10.0f + i,
                IsClosed = false,
                ImageUrl = "https://example.com/image.jpg",
                ImageIsometricUrl = "https://example.com/isometric.jpg"
            };
            var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
            createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Act - Sort by name ascending with name filter
        var response = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?sortBy=name&pageSize=25&name={uniquePrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containersResponse = await response.Content.ReadFromJsonAsync<ContainersResponse>(cancellationToken: cts);
        containersResponse.ShouldNotBeNull();

        var returnedContainers = containersResponse.Items.Where(c => containerNames.Contains(c.Name)).ToList();
        returnedContainers.Count.ShouldBe(3);

        // Should be sorted A, B, C (alphabetically)
        for (var i = 0; i < returnedContainers.Count - 1; i++)
        {
            string.Compare(returnedContainers[i].Name, returnedContainers[i + 1].Name, StringComparison.OrdinalIgnoreCase)
                .ShouldBeLessThanOrEqualTo(0);
        }
    }

    [Fact]
    public async Task GetAllContainers_WithSortByVolumeDescending_ShouldReturnSortedResults()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var volumes = new[] { 10.0f, 30.0f, 20.0f };
        var uniquePrefix = $"VolumeSort-{Guid.NewGuid():N}";
        var containerNames = new List<string>();

        for (var i = 0; i < 3; i++)
        {
            var containerName = $"{uniquePrefix}-{i}";
            containerNames.Add(containerName);
            var createRequest = new ContainersEndpoints.Add.AddContainerRequest
            {
                Name = containerName,
                Description = $"Volume sort test container {i}",
                Volume = volumes[i],
                IsClosed = false,
                ImageUrl = "https://example.com/image.jpg",
                ImageIsometricUrl = "https://example.com/isometric.jpg"
            };
            var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
            createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Act - Sort by volume descending with name filter
        var response = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?sortBy=-volume&pageSize=25&name={uniquePrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containersResponse = await response.Content.ReadFromJsonAsync<ContainersResponse>(cancellationToken: cts);
        containersResponse.ShouldNotBeNull();

        var returnedContainers = containersResponse.Items.Where(c => containerNames.Contains(c.Name)).ToList();
        returnedContainers.Count.ShouldBe(3);

        // Should be sorted by volume descending: 30, 20, 10
        for (var i = 0; i < returnedContainers.Count - 1; i++)
        {
            returnedContainers[i].Volume.ShouldBeGreaterThanOrEqualTo(returnedContainers[i + 1].Volume);
        }
    }

    [Fact]
    public async Task GetAllContainers_WithInvalidPageSize_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?pageSize=0", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllContainers_WithInvalidSortField_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?sortBy=invalidField", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllContainers_WithLargePageSize_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?pageSize=100", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllContainers_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act - Use a unique name that won't match any existing containers
        var uniqueFilter = $"NonExistent-{Guid.NewGuid():N}";
        var response = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?name={uniqueFilter}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containersResponse = await response.Content.ReadFromJsonAsync<ContainersResponse>(cancellationToken: cts);
        containersResponse.ShouldNotBeNull();
        containersResponse.Items.Count().ShouldBe(0);
        containersResponse.Total.ShouldBe(0);
    }
    //TODO
    //[Fact]
    //public async Task GetAllContainers_WithClosedFilter_ShouldReturnOnlyClosedContainers()
    //{
    //    // Arrange
    //    var cts = TestContext.Current.CancellationToken;
    //    await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
    //    using var client = CreateHttpClient("api");

    //    // Create containers with different closed states
    //    var uniquePrefix = $"ClosedFilter-{Guid.NewGuid():N}";
    //    var closedContainerName = $"{uniquePrefix}-Closed";
    //    var openContainerName = $"{uniquePrefix}-Open";

    //    // Create closed container
    //    var closedRequest = new ContainersEndpoints.Add.AddContainerRequest
    //    {
    //        Name = closedContainerName,
    //        Volume = 10.0f,
    //        IsClosed = true
    //    };
    //    await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, closedRequest, cts);

    //    // Create open container
    //    var openRequest = new ContainersEndpoints.Add.AddContainerRequest
    //    {
    //        Name = openContainerName,
    //        Volume = 20.0f,
    //        IsClosed = false
    //    };
    //    await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, openRequest, cts);

    //    // Act - Filter for closed containers only
    //    var response = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?isClosed=true&name={uniquePrefix}", cts);

    //    // Assert
    //    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    //    var containersResponse = await response.Content.ReadFromJsonAsync<ContainersResponse>(cancellationToken: cts);
    //    containersResponse.ShouldNotBeNull();
    //    var filteredContainers = containersResponse.Items.Where(c => c.Name.Contains(uniquePrefix));
    //    filteredContainers.All(c => c.IsClosed).ShouldBeTrue();
    //    filteredContainers.ShouldContain(c => c.Name == closedContainerName);
    //    filteredContainers.ShouldNotContain(c => c.Name == openContainerName);
    //}

    [Fact]
    public async Task GetAllContainers_ReturnsCorrectContentType()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync(ApiEndpoints.Containers.GetAll, cts);

        // Assert
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 3)]
    [InlineData(1, 10)]
    public async Task GetAllContainers_VariousPaginationParameters_ShouldReturnCorrectPage(int page, int pageSize)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create several containers
        var uniquePrefix = $"Pagination-{Guid.NewGuid():N}";
        for (var i = 0; i < 10; i++)
        {
            var createRequest = new ContainersEndpoints.Add.AddContainerRequest
            {
                Name = $"{uniquePrefix}-{i:D2}",
                Volume = (i + 1) * 5.0f,
                IsClosed = i % 2 == 0
            };
            await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        }

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?page={page}&pageSize={pageSize}&name={uniquePrefix}", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containersResponse = await response.Content.ReadFromJsonAsync<ContainersResponse>(cancellationToken: cts);
        containersResponse.ShouldNotBeNull();
        containersResponse.Page.ShouldBe(page);
        containersResponse.PageSize.ShouldBe(pageSize);
        containersResponse.Items.Count().ShouldBeLessThanOrEqualTo(pageSize);
        containersResponse.Total.ShouldBeGreaterThanOrEqualTo(10);
    }

    [Fact]
    public async Task GetAllContainers_InvalidPageParameter_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var response = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?page=0", cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}