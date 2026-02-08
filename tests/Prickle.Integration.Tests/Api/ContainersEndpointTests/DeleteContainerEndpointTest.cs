using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Containers;
using ContainersEndpoints = Prickle.Api.Endpoints.Containers;

namespace Prickle.Integration.Tests.Api.ContainersEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class DeleteContainerEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public DeleteContainerEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task DeleteContainer_ExistingId_ShouldReturnNoContent()
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
        var url = ApiEndpoints.Containers.Delete.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteContainer_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var nonExistentId = Guid.NewGuid();

        // Act
        var url = ApiEndpoints.Containers.Delete.Replace("{id:guid}", nonExistentId.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteContainer_InvalidGuid_ShouldReturnNotFound()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var url = ApiEndpoints.Containers.Delete.Replace("{id:guid}", "invalid-guid");
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteContainer_EmptyGuid_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Act
        var url = ApiEndpoints.Containers.Delete.Replace("{id:guid}", Guid.Empty.ToString());
        var response = await client.DeleteAsync(url, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteContainer_VerifyContainerIsActuallyDeleted()
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

        // Act - Delete the container
        var deleteUrl = ApiEndpoints.Containers.Delete.Replace("{id:guid}", createdContainer!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Assert - Verify container is actually deleted by trying to get it
        var getUrl = ApiEndpoints.Containers.Get.Replace("{id:guid}", createdContainer.Id.ToString());
        var getResponse = await client.GetAsync(getUrl, cts);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteContainer_DeletedContainerNotInList()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a container with unique name
        var uniqueName = $"DeleteTest-{Guid.NewGuid():N}";
        var createRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = uniqueName,
            Volume = 5.0f,
            IsClosed = false
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        var createdContainer = await createResponse.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);

        // Act - Delete the container
        var deleteUrl = ApiEndpoints.Containers.Delete.Replace("{id:guid}", createdContainer!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Assert - Verify container is not in the list anymore
        var listResponse = await client.GetAsync($"{ApiEndpoints.Containers.GetAll}?name={uniqueName}", cts);
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containersResponse = await listResponse.Content.ReadFromJsonAsync<ContainersResponse>(cancellationToken: cts);
        containersResponse.ShouldNotBeNull();
        containersResponse.Items.ShouldNotContain(c => c.Name == uniqueName);
    }

    [Fact]
    public async Task DeleteContainer_TryToDeleteTwice_SecondDeleteShouldReturnBadRequest()
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

        var deleteUrl = ApiEndpoints.Containers.Delete.Replace("{id:guid}", createdContainer!.Id.ToString());
        
        // Act - Delete the container first time
        var firstDeleteResponse = await client.DeleteAsync(deleteUrl, cts);
        firstDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Act - Try to delete the same container again
        var secondDeleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Assert
        secondDeleteResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("minimal-container")]
    [InlineData("complex-container-with-all-fields")]
    public async Task DeleteContainer_DifferentContainerTypes_ShouldReturnNoContent(string containerType)
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var createRequest = containerType == "minimal-container" 
            ? new ContainersEndpoints.Add.AddContainerRequest
            {
                Name = $"{containerType}-{Guid.NewGuid():N}",
                Volume = 1.0f,
                IsClosed = false
            }
            : new ContainersEndpoints.Add.AddContainerRequest
            {
                Name = $"{containerType}-{Guid.NewGuid():N}",
                Description = _faker.Lorem.Sentence(),
                Volume = _faker.Random.Float(1.0f, 100.0f),
                IsClosed = _faker.Random.Bool(),
                ImageUrl = "https://example.com/image.jpg",
                ImageIsometricUrl = "https://example.com/isometric.jpg"
            };

        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        var createdContainer = await createResponse.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);

        // Act
        var deleteUrl = ApiEndpoints.Containers.Delete.Replace("{id:guid}", createdContainer!.Id.ToString());
        var deleteResponse = await client.DeleteAsync(deleteUrl, cts);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}