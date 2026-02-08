using System.Net.Http.Json;
using Bogus;
using Prickle.Api.Endpoints;
using Prickle.Application.Containers;
using ContainersEndpoints = Prickle.Api.Endpoints.Containers;

namespace Prickle.Integration.Tests.Api.ContainersEndpointTests;

[Collection(nameof(ApiTestsCollectionMarker))]
public sealed class UpdateContainerEndpointTest : ApiBaseIntegrationTest
{
    private readonly Faker _faker;

    public UpdateContainerEndpointTest(AppHostFixture appHostFactory) : base(appHostFactory)
    {
        _faker = new Faker();
    }

    [Fact]
    public async Task UpdateContainer_ValidRequest_ShouldReturnOkWithUpdatedContainer()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create a container first
        var originalName = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}";
        var createRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = originalName,
            Description = _faker.Lorem.Sentence(),
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = false,
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        var createdContainer = await createResponse.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);

        var updateRequest = new ContainersEndpoints.Update.UpdateContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-Updated-{Guid.NewGuid():N}",
            Description = "Updated description",
            Volume = 25.5f,
            IsClosed = true,
            ImageUrl = "https://example.com/updated-image.jpg",
            ImageIsometricUrl = "https://example.com/updated-isometric.jpg"
        };

        // Act
        var url = ApiEndpoints.Containers.Update.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.PutAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Id.ShouldBe(createdContainer.Id);
        containerResponse.Name.ShouldBe(updateRequest.Name);
        containerResponse.Description.ShouldBe(updateRequest.Description);
        containerResponse.Volume.ShouldBe(updateRequest.Volume);
        containerResponse.IsClosed.ShouldBe(updateRequest.IsClosed);
        containerResponse.ImageUrl.ShouldBe(updateRequest.ImageUrl);
        containerResponse.ImageIsometricUrl.ShouldBe(updateRequest.ImageIsometricUrl);
    }

    [Fact]
    public async Task UpdateContainer_NonExistentId_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var nonExistentId = Guid.NewGuid();
        var updateRequest = new ContainersEndpoints.Update.UpdateContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Description = _faker.Lorem.Sentence(),
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };

        // Act
        var url = ApiEndpoints.Containers.Update.Replace("{id:guid}", nonExistentId.ToString());
        var response = await client.PutAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateContainer_EmptyName_ShouldReturnBadRequest()
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

        var updateRequest = new ContainersEndpoints.Update.UpdateContainerRequest
        {
            Name = string.Empty,
            Description = _faker.Lorem.Sentence(),
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };

        // Act
        var url = ApiEndpoints.Containers.Update.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.PutAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateContainer_InvalidVolume_ShouldReturnBadRequest()
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

        var updateRequest = new ContainersEndpoints.Update.UpdateContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-Updated-{Guid.NewGuid():N}",
            Description = _faker.Lorem.Sentence(),
            Volume = -5.0f,
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };

        // Act
        var url = ApiEndpoints.Containers.Update.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.PutAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateContainer_DuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        // Create first container
        var firstName = $"{_faker.Lorem.Word()}-First-{Guid.NewGuid():N}";
        var firstRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = firstName,
            Description = _faker.Lorem.Sentence(),
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image1.jpg",
            ImageIsometricUrl = "https://example.com/isometric1.jpg"
        };
        await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, firstRequest, cts);

        // Create second container
        var secondName = $"{_faker.Lorem.Word()}-Second-{Guid.NewGuid():N}";
        var secondRequest = new ContainersEndpoints.Add.AddContainerRequest
        {
            Name = secondName,
            Description = _faker.Lorem.Sentence(),
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/image2.jpg",
            ImageIsometricUrl = "https://example.com/isometric2.jpg"
        };
        var secondResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, secondRequest, cts);
        var secondContainer = await secondResponse.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);

        // Try to update second container to have the same name as the first
        var updateRequest = new ContainersEndpoints.Update.UpdateContainerRequest
        {
            Name = firstName, // This should cause a conflict
            Description = "Updated description",
            Volume = _faker.Random.Float(1.0f, 100.0f),
            IsClosed = _faker.Random.Bool(),
            ImageUrl = "https://example.com/updated-image.jpg",
            ImageIsometricUrl = "https://example.com/updated-isometric.jpg"
        };

        // Act
        var url = ApiEndpoints.Containers.Update.Replace("{id:guid}", secondContainer!.Id.ToString());
        var response = await client.PutAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateContainer_WithNullOptionalFields_ShouldReturnOk()
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
            IsClosed = false,
            ImageUrl = "https://example.com/image.jpg",
            ImageIsometricUrl = "https://example.com/isometric.jpg"
        };
        var createResponse = await client.PostAsJsonAsync(ApiEndpoints.Containers.Add, createRequest, cts);
        var createdContainer = await createResponse.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);

        var updateRequest = new ContainersEndpoints.Update.UpdateContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-Updated-{Guid.NewGuid():N}",
            Description = null,
            Volume = 15.0f,
            IsClosed = true,
            ImageUrl = null,
            ImageIsometricUrl = null
        };

        // Act
        var url = ApiEndpoints.Containers.Update.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.PutAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Name.ShouldBe(updateRequest.Name);
        containerResponse.Description.ShouldBeNull();
        containerResponse.Volume.ShouldBe(updateRequest.Volume);
        containerResponse.IsClosed.ShouldBe(updateRequest.IsClosed);
        containerResponse.ImageUrl.ShouldBeNull();
        containerResponse.ImageIsometricUrl.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateContainer_InvalidGuidFormat_ShouldReturnNotFound()
    {
        // Arrange
        var cts = TestContext.Current.CancellationToken;
        await AppHostFactory.App.StartAsync(cts).WaitAsync(AppHostFixture.StartStopTimeout, cts);
        using var client = CreateHttpClient("api");

        var updateRequest = new ContainersEndpoints.Update.UpdateContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-{Guid.NewGuid():N}",
            Volume = 5.0f,
            IsClosed = false
        };

        // Act
        var url = ApiEndpoints.Containers.Update.Replace("{id:guid}", "invalid-guid");
        var response = await client.PutAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateContainer_NameTooLong_ShouldReturnBadRequest()
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

        var longName = _faker.Random.String2(256); // Max is 255
        var updateRequest = new ContainersEndpoints.Update.UpdateContainerRequest
        {
            Name = longName,
            Volume = 10.0f,
            IsClosed = true
        };

        // Act
        var url = ApiEndpoints.Containers.Update.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.PutAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateContainer_NameWithWhitespace_ShouldTrimAndReturnOk()
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

        var newName = $"{_faker.Lorem.Word()}-Updated-{Guid.NewGuid():N}";
        var updateRequest = new ContainersEndpoints.Update.UpdateContainerRequest
        {
            Name = $"  {newName}  ",
            Volume = 10.0f,
            IsClosed = true
        };

        // Act
        var url = ApiEndpoints.Containers.Update.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.PutAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Name.ShouldBe(newName);
    }

    [Theory]
    [InlineData(0.1f, true)]
    [InlineData(25.5f, false)]
    [InlineData(999.9f, true)]
    public async Task UpdateContainer_VariousVolumeAndClosedStates_ShouldReturnOk(float volume, bool isClosed)
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

        var updateRequest = new ContainersEndpoints.Update.UpdateContainerRequest
        {
            Name = $"{_faker.Lorem.Word()}-Updated-{Guid.NewGuid():N}",
            Volume = volume,
            IsClosed = isClosed
        };

        // Act
        var url = ApiEndpoints.Containers.Update.Replace("{id:guid}", createdContainer!.Id.ToString());
        var response = await client.PutAsJsonAsync(url, updateRequest, cts);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var containerResponse = await response.Content.ReadFromJsonAsync<ContainerResponse>(cancellationToken: cts);
        containerResponse.ShouldNotBeNull();
        containerResponse.Volume.ShouldBe(volume);
        containerResponse.IsClosed.ShouldBe(isClosed);
    }
}