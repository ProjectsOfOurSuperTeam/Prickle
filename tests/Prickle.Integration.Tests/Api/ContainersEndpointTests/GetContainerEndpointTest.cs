using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Containers;
using ContainersEndpoints = Prickle.Api.Endpoints.Containers;

namespace Prickle.Integration.Tests.Api.ContainersEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class GetContainerEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public GetContainerEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task GetContainer_ExistingId_ShouldReturnOkWithContainer()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a container first
        var containerName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = containerName,
            Description = _faker.Lorem.Sentence(),
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        var createdContainer = await createResponse.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Containers.Get.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Id.ShouldBe(createdContainer.Id);
        containerResponse.Name.ShouldBe(containerName);
        containerResponse.Description.ShouldBe(createRequest.Description);
        containerResponse.Volume.ShouldBe(createRequest.Volume);
        containerResponse.IsClosed.ShouldBe(createRequest.IsClosed);
        containerResponse.ImageUrl.ShouldBe(createRequest.ImageUrl);
        containerResponse.ImageIsometricUrl.ShouldBe(createRequest.ImageIsometricUrl);
    }

    [Fact]
    public async Task GetContainer_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var nonExistentId = Guid.NewGuid();

        // Act
        var url = ApiEndpoints.Containers.Get.Replace("{id:guid}", nonExistentId.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetContainer_InvalidGuid_ShouldReturnNotFound()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var url = ApiEndpoints.Containers.Get.Replace("{id:guid}", "invalid-guid");
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetContainer_EmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var url = ApiEndpoints.Containers.Get.Replace("{id:guid}", Guid.Empty.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetContainer_ReturnsCorrectContentType()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a container first
        var containerName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = containerName,
            Volume = 5.0f,
            IsClosed = false
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        var createdContainer = await createResponse.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Containers.Get.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    [Fact]
    public async Task GetContainer_MinimalContainer_ShouldReturnOk()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a minimal container first
        var containerName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = containerName,
            Volume = 1.0f,
            IsClosed = false
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        var createdContainer = await createResponse.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Containers.Get.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Name.ShouldBe(containerName);
        containerResponse.Volume.ShouldBe(1.0f);
        containerResponse.IsClosed.ShouldBe(false);
    }

    [Theory]
    [InlineData(1.0f, true)]
    [InlineData(5.5f, false)]
    [InlineData(100.0f, true)]
    [InlineData(0.1f, false)]
    public async Task GetContainer_VariousVolumeAndClosedStates_ShouldReturnOk(float volume, bool isClosed)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a container with specific volume and closed state
        var containerName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = containerName,
            Volume = volume,
            IsClosed = isClosed,
            Description = _faker.Lorem.Sentence()
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        var createdContainer = await createResponse.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Containers.Get.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Volume.ShouldBe(volume);
        containerResponse.IsClosed.ShouldBe(isClosed);
    }

    [Fact]
    public async Task GetContainer_WithNullOptionalFields_ShouldReturnOkWithContainer()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a container with null optional fields
        var containerName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = containerName,
            Description = null,
            Volume = 10.5f,
            IsClosed = true,
            ImageUrl = null,
            ImageIsometricUrl = null
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        var createdContainer = await createResponse.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);

        // Act
        var url = ApiEndpoints.Containers.Get.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.GetAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Id.ShouldBe(createdContainer.Id);
        containerResponse.Name.ShouldBe(containerName);
        containerResponse.Description.ShouldBeNull();
        containerResponse.Volume.ShouldBe(10.5f);
        containerResponse.IsClosed.ShouldBe(true);
        containerResponse.ImageUrl.ShouldBeNull();
        containerResponse.ImageIsometricUrl.ShouldBeNull();
    }
}